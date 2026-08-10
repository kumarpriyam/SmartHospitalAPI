using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace SmartHospitalAPI.Controllers
{
    [ApiController]
    [Route("api/queue")]
    public class QueueController : ControllerBase
    {
        private readonly NpgsqlConnection _connection;

        public QueueController(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        // =====================================================
        // GET WAITING QUEUE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetWaitingQueue()
        {
            try
            {
                await _connection.OpenAsync();

                string query = @"
                    SELECT
                        token,
                        name,
                        age,
                        priority,
                        type,
                        department,
                        status,
                        assigned_doctor_id,
                        appointment_date,
                        appointment_time,
                        has_appointment
                    FROM patients
                    WHERE status = 'WAITING'
                    ORDER BY priority DESC,
                             has_appointment DESC,
                             token ASC;
                ";

                using var command = new NpgsqlCommand(query, _connection);
                using var reader = await command.ExecuteReaderAsync();

                var queue = new List<object>();

                int position = 1;

                while (await reader.ReadAsync())
                {
                    queue.Add(new
                    {
                        position = position++,
                        token = reader.GetInt32(0),
                        name = reader.GetString(1),
                        age = reader.GetInt32(2),
                        priority = reader.GetInt32(3),
                        type = reader.GetString(4),
                        department = reader.GetString(5),
                        status = reader.GetString(6),
                        assignedDoctorId = reader.GetInt32(7),
                        appointmentDate = reader.GetString(8),
                        appointmentTime = reader.GetString(9),
                        hasAppointment = reader.GetBoolean(10)
                    });
                }

                return Ok(new
                {
                    success = true,
                    totalWaitingPatients = queue.Count,
                    queue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }


        // =====================================================
        // CALL NEXT PATIENT
        // =====================================================

        [HttpPost("call-next")]
        public async Task<IActionResult> CallNextPatient()
        {
            try
            {
                await _connection.OpenAsync();

                // -------------------------------------------------
                // Find highest priority waiting patient
                // -------------------------------------------------

                string patientQuery = @"
                    SELECT
                        token,
                        name,
                        age,
                        priority,
                        type,
                        department
                    FROM patients
                    WHERE status = 'WAITING'
                    ORDER BY priority DESC,
                             has_appointment DESC,
                             token ASC
                    LIMIT 1;
                ";

                using var patientCommand =
                    new NpgsqlCommand(patientQuery, _connection);

                using var patientReader =
                    await patientCommand.ExecuteReaderAsync();

                if (!await patientReader.ReadAsync())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No patient is waiting."
                    });
                }

                int token = patientReader.GetInt32(0);
                string patientName = patientReader.GetString(1);
                int age = patientReader.GetInt32(2);
                int priority = patientReader.GetInt32(3);
                string type = patientReader.GetString(4);
                string department = patientReader.GetString(5);

                await patientReader.CloseAsync();

                // -------------------------------------------------
                // Find available doctor for department
                // -------------------------------------------------

                string doctorQuery = @"
                    SELECT
                        doctor_id,
                        name,
                        specialization
                    FROM doctors
                    WHERE specialization = @department
                      AND available = TRUE
                    ORDER BY doctor_id
                    LIMIT 1;
                ";

                using var doctorCommand =
                    new NpgsqlCommand(doctorQuery, _connection);

                doctorCommand.Parameters.AddWithValue(
                    "@department",
                    department
                );

                using var doctorReader =
                    await doctorCommand.ExecuteReaderAsync();

                if (!await doctorReader.ReadAsync())
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"No available doctor for {department}.",
                        patient = new
                        {
                            token,
                            name = patientName,
                            department,
                            status = "WAITING"
                        }
                    });
                }

                int doctorId = doctorReader.GetInt32(0);
                string doctorName = doctorReader.GetString(1);
                string specialization = doctorReader.GetString(2);

                await doctorReader.CloseAsync();

                // -------------------------------------------------
                // Update patient
                // -------------------------------------------------

                string updatePatient = @"
                    UPDATE patients
                    SET
                        status = 'WITH DOCTOR',
                        assigned_doctor_id = @doctorId
                    WHERE token = @token;
                ";

                using var updatePatientCommand =
                    new NpgsqlCommand(updatePatient, _connection);

                updatePatientCommand.Parameters.AddWithValue(
                    "@doctorId",
                    doctorId
                );

                updatePatientCommand.Parameters.AddWithValue(
                    "@token",
                    token
                );

                await updatePatientCommand.ExecuteNonQueryAsync();

                // -------------------------------------------------
                // Update doctor
                // -------------------------------------------------

                string updateDoctor = @"
                    UPDATE doctors
                    SET
                        available = FALSE,
                        current_patient_token = @token
                    WHERE doctor_id = @doctorId;
                ";

                using var updateDoctorCommand =
                    new NpgsqlCommand(updateDoctor, _connection);

                updateDoctorCommand.Parameters.AddWithValue(
                    "@token",
                    token
                );

                updateDoctorCommand.Parameters.AddWithValue(
                    "@doctorId",
                    doctorId
                );

                await updateDoctorCommand.ExecuteNonQueryAsync();

                // -------------------------------------------------
                // SUCCESS
                // -------------------------------------------------

                return Ok(new
                {
                    success = true,
                    message = "Next patient called successfully!",

                    patient = new
                    {
                        token,
                        name = patientName,
                        age,
                        priority,
                        type,
                        department,
                        status = "WITH DOCTOR",
                        assignedDoctorId = doctorId
                    },

                    doctor = new
                    {
                        doctorId,
                        name = doctorName,
                        specialization,
                        status = "BUSY"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }


        // =====================================================
        // COMPLETE CONSULTATION
        // =====================================================

        [HttpPost("complete/{token}")]
        public async Task<IActionResult> CompleteConsultation(int token)
        {
            try
            {
                await _connection.OpenAsync();

                // -------------------------------------------------
                // Find patient
                // -------------------------------------------------

                string patientQuery = @"
                    SELECT
                        name,
                        status,
                        assigned_doctor_id
                    FROM patients
                    WHERE token = @token;
                ";

                using var patientCommand =
                    new NpgsqlCommand(patientQuery, _connection);

                patientCommand.Parameters.AddWithValue(
                    "@token",
                    token
                );

                using var reader =
                    await patientCommand.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Patient token not found."
                    });
                }

                string patientName = reader.GetString(0);
                string status = reader.GetString(1);
                int doctorId = reader.GetInt32(2);

                await reader.CloseAsync();

                if (status != "WITH DOCTOR")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Patient is not currently with a doctor.",
                        currentStatus = status
                    });
                }

                // -------------------------------------------------
                // Update patient
                // -------------------------------------------------

                string updatePatient = @"
                    UPDATE patients
                    SET
                        status = 'COMPLETED'
                    WHERE token = @token;
                ";

                using var updatePatientCommand =
                    new NpgsqlCommand(updatePatient, _connection);

                updatePatientCommand.Parameters.AddWithValue(
                    "@token",
                    token
                );

                await updatePatientCommand.ExecuteNonQueryAsync();

                // -------------------------------------------------
                // Update doctor
                // -------------------------------------------------

                string updateDoctor = @"
                    UPDATE doctors
                    SET
                        available = TRUE,
                        current_patient_token = -1,
                        consultations_completed =
                            consultations_completed + 1
                    WHERE doctor_id = @doctorId;
                ";

                using var updateDoctorCommand =
                    new NpgsqlCommand(updateDoctor, _connection);

                updateDoctorCommand.Parameters.AddWithValue(
                    "@doctorId",
                    doctorId
                );

                await updateDoctorCommand.ExecuteNonQueryAsync();

                // -------------------------------------------------
                // Consultation history
                // -------------------------------------------------

                string historyQuery = @"
                    INSERT INTO consultation_history
                    (
                        doctor_id,
                        patient_token
                    )
                    VALUES
                    (
                        @doctorId,
                        @token
                    );
                ";

                using var historyCommand =
                    new NpgsqlCommand(historyQuery, _connection);

                historyCommand.Parameters.AddWithValue(
                    "@doctorId",
                    doctorId
                );

                historyCommand.Parameters.AddWithValue(
                    "@token",
                    token
                );

                await historyCommand.ExecuteNonQueryAsync();

                // -------------------------------------------------
                // SUCCESS
                // -------------------------------------------------

                return Ok(new
                {
                    success = true,
                    message = "Consultation completed successfully!",

                    patient = new
                    {
                        token,
                        name = patientName,
                        status = "COMPLETED"
                    },

                    doctor = new
                    {
                        doctorId,
                        status = "AVAILABLE"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
}

