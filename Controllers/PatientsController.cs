using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace SmartHospitalAPI.Controllers
{
    [ApiController]
    [Route("api/patients")]
    public class PatientsController : ControllerBase
    {
        private readonly NpgsqlConnection _connection;

        public PatientsController(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        // =====================================================
        // GET ALL PATIENTS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            try
            {
                await _connection.OpenAsync();

                var patients = new List<object>();

                string query = @"
                    SELECT
                        token,
                        name,
                        mobile,
                        gender,
                        age,
                        location,
                        priority,
                        type,
                        department,
                        status,
                        assigned_doctor_id,
                        appointment_date,
                        appointment_time,
                        has_appointment,
                        appointment_cancelled
                    FROM patients
                    ORDER BY token;
                ";

                using var command = new NpgsqlCommand(query, _connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    patients.Add(CreatePatientObject(reader));
                }

                return Ok(new
                {
                    success = true,
                    count = patients.Count,
                    patients = patients
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: ex.Message,
                    title: "Failed to fetch patients"
                );
            }
            finally
            {
                await CloseConnection();
            }
        }


        // =====================================================
        // GET PATIENT BY TOKEN
        // =====================================================

        [HttpGet("{token:int}")]
        public async Task<IActionResult> GetPatient(int token)
        {
            try
            {
                await _connection.OpenAsync();

                string query = @"
                    SELECT
                        token,
                        name,
                        mobile,
                        gender,
                        age,
                        location,
                        priority,
                        type,
                        department,
                        status,
                        assigned_doctor_id,
                        appointment_date,
                        appointment_time,
                        has_appointment,
                        appointment_cancelled
                    FROM patients
                    WHERE token = @token;
                ";

                using var command = new NpgsqlCommand(query, _connection);
                command.Parameters.AddWithValue("@token", token);

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Patient not found!"
                    });
                }

                return Ok(new
                {
                    success = true,
                    patient = CreatePatientObject(reader)
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: ex.Message,
                    title: "Failed to fetch patient"
                );
            }
            finally
            {
                await CloseConnection();
            }
        }


        // =====================================================
        // REGISTER PATIENT
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> RegisterPatient(
            [FromBody] PatientRequest request)
        {
            try
            {
                // -----------------------------
                // VALIDATION
                // -----------------------------

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Patient name is required!"
                    });
                }

                if (request.Age <= 0 || request.Age > 120)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please enter a valid age!"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Mobile))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mobile number is required!"
                    });
                }

                string mobile = request.Mobile.Trim();

                if (mobile.Length != 10 ||
                    !mobile.All(char.IsDigit))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please enter a valid 10 digit mobile number!"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Department))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Department is required!"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Type))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Patient type is required!"
                    });
                }

                await _connection.OpenAsync();

                // =================================================
                // FIND NEXT TOKEN
                // =================================================

                string tokenQuery = @"
                    SELECT COALESCE(MAX(token), 1000) + 1
                    FROM patients;
                ";

                using var tokenCommand =
                    new NpgsqlCommand(tokenQuery, _connection);

                int token = Convert.ToInt32(
                    await tokenCommand.ExecuteScalarAsync()
                );

                // =================================================
                // PRIORITY
                // =================================================

                int priority =
                    request.Type.Trim().ToLower() == "emergency"
                    ? 2
                    : 1;

                // =================================================
                // INSERT PATIENT
                // =================================================

                string insertQuery = @"
                    INSERT INTO patients
                    (
                        token,
                        name,
                        mobile,
                        gender,
                        age,
                        location,
                        priority,
                        type,
                        department,
                        status,
                        assigned_doctor_id,
                        appointment_date,
                        appointment_time,
                        has_appointment,
                        appointment_cancelled
                    )
                    VALUES
                    (
                        @token,
                        @name,
                        @mobile,
                        @gender,
                        @age,
                        @location,
                        @priority,
                        @type,
                        @department,
                        'WAITING',
                        -1,
                        '',
                        '',
                        FALSE,
                        FALSE
                    );
                ";

                using var command =
                    new NpgsqlCommand(insertQuery, _connection);

                command.Parameters.AddWithValue(
                    "@token",
                    token
                );

                command.Parameters.AddWithValue(
                    "@name",
                    request.Name.Trim()
                );

                command.Parameters.AddWithValue(
                    "@mobile",
                    mobile
                );

                command.Parameters.AddWithValue(
                    "@gender",
                    request.Gender?.Trim() ?? ""
                );

                command.Parameters.AddWithValue(
                    "@age",
                    request.Age
                );

                command.Parameters.AddWithValue(
                    "@location",
                    request.Location?.Trim() ?? ""
                );

                command.Parameters.AddWithValue(
                    "@priority",
                    priority
                );

                command.Parameters.AddWithValue(
                    "@type",
                    request.Type.Trim()
                );

                command.Parameters.AddWithValue(
                    "@department",
                    request.Department.Trim()
                );

                await command.ExecuteNonQueryAsync();

                // =================================================
                // RESPONSE
                // =================================================

                return Ok(new
                {
                    success = true,
                    message = "Patient registered successfully!",
                    patient = new
                    {
                        token = token,
                        name = request.Name.Trim(),
                        mobile = mobile,
                        gender = request.Gender?.Trim() ?? "",
                        age = request.Age,
                        location = request.Location?.Trim() ?? "",
                        priority = priority,
                        type = request.Type.Trim(),
                        department = request.Department.Trim(),
                        status = "WAITING",
                        assignedDoctorId = -1,
                        appointmentDate = "",
                        appointmentTime = "",
                        hasAppointment = false,
                        appointmentCancelled = false
                    }
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: ex.Message,
                    title: "Patient registration failed"
                );
            }
            finally
            {
                await CloseConnection();
            }
        }


        // =====================================================
        // UPDATE PATIENT
        // =====================================================

        [HttpPut("{token:int}")]
        public async Task<IActionResult> UpdatePatient(
            int token,
            [FromBody] UpdatePatientRequest request)
        {
            try
            {
                // -----------------------------
                // VALIDATION
                // -----------------------------

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Patient name is required!"
                    });
                }

                if (request.Age <= 0 || request.Age > 120)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please enter a valid age!"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Mobile))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mobile number is required!"
                    });
                }

                string mobile = request.Mobile.Trim();

                if (mobile.Length != 10 ||
                    !mobile.All(char.IsDigit))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please enter a valid 10 digit mobile number!"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Department))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Department is required!"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Type))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Patient type is required!"
                    });
                }

                await _connection.OpenAsync();

                // =================================================
                // PRIORITY
                // =================================================

                int priority =
                    request.Type.Trim().ToLower() == "emergency"
                    ? 2
                    : 1;

                // =================================================
                // UPDATE
                // =================================================

                string query = @"
                    UPDATE patients
                    SET
                        name = @name,
                        mobile = @mobile,
                        gender = @gender,
                        age = @age,
                        location = @location,
                        department = @department,
                        type = @type,
                        priority = @priority
                    WHERE token = @token;
                ";

                using var command =
                    new NpgsqlCommand(query, _connection);

                command.Parameters.AddWithValue(
                    "@token",
                    token
                );

                command.Parameters.AddWithValue(
                    "@name",
                    request.Name.Trim()
                );

                command.Parameters.AddWithValue(
                    "@mobile",
                    mobile
                );

                command.Parameters.AddWithValue(
                    "@gender",
                    request.Gender?.Trim() ?? ""
                );

                command.Parameters.AddWithValue(
                    "@age",
                    request.Age
                );

                command.Parameters.AddWithValue(
                    "@location",
                    request.Location?.Trim() ?? ""
                );

                command.Parameters.AddWithValue(
                    "@department",
                    request.Department.Trim()
                );

                command.Parameters.AddWithValue(
                    "@type",
                    request.Type.Trim()
                );

                command.Parameters.AddWithValue(
                    "@priority",
                    priority
                );

                int rows =
                    await command.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Patient not found!"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Patient details updated successfully!"
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: ex.Message,
                    title: "Patient update failed"
                );
            }
            finally
            {
                await CloseConnection();
            }
        }


        // =====================================================
        // DELETE PATIENT
        // =====================================================

        [HttpDelete("{token:int}")]
        public async Task<IActionResult> DeletePatient(int token)
        {
            try
            {
                await _connection.OpenAsync();

                string query = @"
                    DELETE FROM patients
                    WHERE token = @token;
                ";

                using var command =
                    new NpgsqlCommand(query, _connection);

                command.Parameters.AddWithValue(
                    "@token",
                    token
                );

                int rows =
                    await command.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Patient not found!"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Patient deleted successfully!"
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: ex.Message,
                    title: "Patient deletion failed"
                );
            }
            finally
            {
                await CloseConnection();
            }
        }


        // =====================================================
        // HELPER - CREATE PATIENT OBJECT
        // =====================================================

        private static object CreatePatientObject(
            NpgsqlDataReader reader)
        {
            return new
            {
                token = reader.GetInt32(0),

                name = reader.IsDBNull(1)
                    ? ""
                    : reader.GetString(1),

                mobile = reader.IsDBNull(2)
                    ? ""
                    : reader.GetString(2),

                gender = reader.IsDBNull(3)
                    ? ""
                    : reader.GetString(3),

                age = reader.IsDBNull(4)
                    ? 0
                    : reader.GetInt32(4),

                location = reader.IsDBNull(5)
                    ? ""
                    : reader.GetString(5),

                priority = reader.IsDBNull(6)
                    ? 1
                    : reader.GetInt32(6),

                type = reader.IsDBNull(7)
                    ? ""
                    : reader.GetString(7),

                department = reader.IsDBNull(8)
                    ? ""
                    : reader.GetString(8),

                status = reader.IsDBNull(9)
                    ? ""
                    : reader.GetString(9),

                assignedDoctorId =
                    reader.IsDBNull(10)
                        ? -1
                        : reader.GetInt32(10),

                appointmentDate =
                    reader.IsDBNull(11)
                        ? ""
                        : reader.GetValue(11).ToString(),

                appointmentTime =
                    reader.IsDBNull(12)
                        ? ""
                        : reader.GetValue(12).ToString(),

                hasAppointment =
                    reader.IsDBNull(13)
                        ? false
                        : reader.GetBoolean(13),

                appointmentCancelled =
                    reader.IsDBNull(14)
                        ? false
                        : reader.GetBoolean(14)
            };
        }


        // =====================================================
        // CLOSE DATABASE CONNECTION
        // =====================================================

        private async Task CloseConnection()
        {
            if (_connection.State ==
                System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }
    }


    // =========================================================
    // REGISTER PATIENT REQUEST MODEL
    // =========================================================

    public class PatientRequest
    {
        public string Name { get; set; } = "";

        public string Mobile { get; set; } = "";

        public string Gender { get; set; } = "";

        public int Age { get; set; }

        public string Location { get; set; } = "";

        public string Type { get; set; } = "";

        public string Department { get; set; } = "";
    }


    // =========================================================
    // UPDATE PATIENT REQUEST MODEL
    // =========================================================

    public class UpdatePatientRequest
    {
        public string Name { get; set; } = "";

        public string Mobile { get; set; } = "";

        public string Gender { get; set; } = "";

        public int Age { get; set; }

        public string Location { get; set; } = "";

        public string Type { get; set; } = "";

        public string Department { get; set; } = "";
    }
}