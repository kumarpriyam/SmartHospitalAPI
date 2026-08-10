using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace SmartHospitalAPI.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public DoctorsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // =========================================================
    // GET ALL DOCTORS
    // GET: /api/doctors
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetDoctors()
    {
        try
        {
            await _connection.OpenAsync();

            string query = @"
                SELECT
                    doctor_id,
                    name,
                    specialization,
                    available,
                    consultations_completed,
                    current_patient_token
                FROM doctors
                ORDER BY doctor_id;
            ";

            using var command = new NpgsqlCommand(query, _connection);
            await using var reader = await command.ExecuteReaderAsync();

            var doctors = new List<object>();

            while (await reader.ReadAsync())
            {
                bool available = reader.GetBoolean(3);

                doctors.Add(new
                {
                    doctorId = reader.GetInt32(0),
                    name = reader.GetString(1),
                    specialization = reader.GetString(2),
                    available = available,
                    consultationsCompleted = reader.GetInt32(4),
                    currentPatientToken = reader.IsDBNull(5)
                        ? -1
                        : reader.GetInt32(5),

                    status = available
                        ? "AVAILABLE"
                        : "BUSY"
                });
            }

            return Ok(new
            {
                success = true,
                totalDoctors = doctors.Count,
                doctors = doctors
            });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to fetch doctors"
            );
        }
        finally
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                await _connection.CloseAsync();
        }
    }


    // =========================================================
    // GET DOCTOR BY ID
    // GET: /api/doctors/101
    // =========================================================

    [HttpGet("{doctorId:int}")]
    public async Task<IActionResult> GetDoctor(int doctorId)
    {
        try
        {
            await _connection.OpenAsync();

            string query = @"
                SELECT
                    doctor_id,
                    name,
                    specialization,
                    available,
                    consultations_completed,
                    current_patient_token
                FROM doctors
                WHERE doctor_id = @id;
            ";

            using var command = new NpgsqlCommand(query, _connection);

            command.Parameters.AddWithValue("@id", doctorId);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound(new
                {
                    success = false,
                    message = "Doctor not found."
                });
            }

            bool available = reader.GetBoolean(3);

            return Ok(new
            {
                success = true,

                doctor = new
                {
                    doctorId = reader.GetInt32(0),
                    name = reader.GetString(1),
                    specialization = reader.GetString(2),
                    available = available,

                    consultationsCompleted = reader.GetInt32(4),

                    currentPatientToken = reader.IsDBNull(5)
                        ? -1
                        : reader.GetInt32(5),

                    status = available
                        ? "AVAILABLE"
                        : "BUSY"
                }
            });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to fetch doctor"
            );
        }
        finally
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                await _connection.CloseAsync();
        }
    }


    // =========================================================
    // ADD DOCTOR
    // POST: /api/doctors
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> AddDoctor(
        [FromBody] DoctorRequest request)
    {
        try
        {
            if (request.DoctorId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please enter a valid Doctor ID."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Doctor name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Specialization))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Specialization is required."
                });
            }

            await _connection.OpenAsync();

            string query = @"
                INSERT INTO doctors
                (
                    doctor_id,
                    name,
                    specialization,
                    available,
                    consultations_completed,
                    current_patient_token
                )
                VALUES
                (
                    @id,
                    @name,
                    @specialization,
                    TRUE,
                    0,
                    -1
                );
            ";

            using var command =
                new NpgsqlCommand(query, _connection);

            command.Parameters.AddWithValue(
                "@id",
                request.DoctorId
            );

            command.Parameters.AddWithValue(
                "@name",
                request.Name.Trim()
            );

            command.Parameters.AddWithValue(
                "@specialization",
                request.Specialization.Trim()
            );

            await command.ExecuteNonQueryAsync();

            return Ok(new
            {
                success = true,
                message = "Doctor added successfully!",

                doctor = new
                {
                    doctorId = request.DoctorId,
                    name = request.Name.Trim(),
                    specialization = request.Specialization.Trim(),
                    available = true,
                    consultationsCompleted = 0,
                    currentPatientToken = -1,
                    status = "AVAILABLE"
                }
            });
        }
        catch (PostgresException ex)
        {
            if (ex.SqlState == "23505")
            {
                return Conflict(new
                {
                    success = false,
                    message = "Doctor ID already exists."
                });
            }

            return Problem(
                detail: ex.Message,
                title: "Failed to add doctor"
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to add doctor"
            );
        }
        finally
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                await _connection.CloseAsync();
        }
    }


    // =========================================================
    // EDIT DOCTOR
    // PUT: /api/doctors/101
    // =========================================================

    [HttpPut("{doctorId:int}")]
    public async Task<IActionResult> UpdateDoctor(
        int doctorId,
        [FromBody] DoctorRequest request)
    {
        try
        {
            if (doctorId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid Doctor ID."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Doctor name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Specialization))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Specialization is required."
                });
            }

            await _connection.OpenAsync();

            string query = @"
                UPDATE doctors
                SET
                    name = @name,
                    specialization = @specialization
                WHERE doctor_id = @id;
            ";

            using var command =
                new NpgsqlCommand(query, _connection);

            command.Parameters.AddWithValue(
                "@id",
                doctorId
            );

            command.Parameters.AddWithValue(
                "@name",
                request.Name.Trim()
            );

            command.Parameters.AddWithValue(
                "@specialization",
                request.Specialization.Trim()
            );

            int rows =
                await command.ExecuteNonQueryAsync();

            if (rows == 0)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Doctor not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Doctor updated successfully!"
            });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to update doctor"
            );
        }
        finally
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                await _connection.CloseAsync();
        }
    }


    // =========================================================
    // CHANGE DOCTOR AVAILABILITY
    // PUT: /api/doctors/101/availability
    // =========================================================

    [HttpPut("{doctorId:int}/availability")]
    public async Task<IActionResult> UpdateAvailability(
        int doctorId,
        [FromBody] DoctorAvailabilityRequest request)
    {
        try
        {
            await _connection.OpenAsync();

            string query = @"
                UPDATE doctors
                SET available = @available
                WHERE doctor_id = @id;
            ";

            using var command =
                new NpgsqlCommand(query, _connection);

            command.Parameters.AddWithValue(
                "@id",
                doctorId
            );

            command.Parameters.AddWithValue(
                "@available",
                request.Available
            );

            int rows =
                await command.ExecuteNonQueryAsync();

            if (rows == 0)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Doctor not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = request.Available
                    ? "Doctor marked as available."
                    : "Doctor marked as busy."
            });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to update doctor availability"
            );
        }
        finally
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                await _connection.CloseAsync();
        }
    }


    // =========================================================
    // DELETE DOCTOR
    // DELETE: /api/doctors/101
    // =========================================================

    [HttpDelete("{doctorId:int}")]
    public async Task<IActionResult> DeleteDoctor(
        int doctorId)
    {
        try
        {
            await _connection.OpenAsync();

            // Do not delete doctor if currently handling a patient
            string checkQuery = @"
                SELECT current_patient_token
                FROM doctors
                WHERE doctor_id = @id;
            ";

            using var checkCommand =
                new NpgsqlCommand(checkQuery, _connection);

            checkCommand.Parameters.AddWithValue(
                "@id",
                doctorId
            );

            object? currentToken =
                await checkCommand.ExecuteScalarAsync();

            if (currentToken == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Doctor not found."
                });
            }

            if (currentToken != DBNull.Value &&
                Convert.ToInt32(currentToken) != -1)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Doctor cannot be deleted while handling a patient."
                });
            }

            string deleteQuery = @"
                DELETE FROM doctors
                WHERE doctor_id = @id;
            ";

            using var deleteCommand =
                new NpgsqlCommand(deleteQuery, _connection);

            deleteCommand.Parameters.AddWithValue(
                "@id",
                doctorId
            );

            int rows =
                await deleteCommand.ExecuteNonQueryAsync();

            if (rows == 0)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Doctor not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Doctor deleted successfully."
            });
        }
        catch (PostgresException ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Doctor cannot be deleted"
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to delete doctor"
            );
        }
        finally
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                await _connection.CloseAsync();
        }
    }


    // =========================================================
    // DOCTOR CONSULTATION HISTORY
    // GET: /api/doctors/102/history
    // =========================================================

    [HttpGet("{doctorId:int}/history")]
    public async Task<IActionResult> GetDoctorHistory(
        int doctorId)
    {
        try
        {
            await _connection.OpenAsync();

            string doctorQuery = @"
                SELECT
                    name,
                    specialization,
                    consultations_completed
                FROM doctors
                WHERE doctor_id = @id;
            ";

            using var doctorCommand =
                new NpgsqlCommand(
                    doctorQuery,
                    _connection
                );

            doctorCommand.Parameters.AddWithValue(
                "@id",
                doctorId
            );

            await using var doctorReader =
                await doctorCommand.ExecuteReaderAsync();

            if (!await doctorReader.ReadAsync())
            {
                return NotFound(new
                {
                    success = false,
                    message = "Doctor not found."
                });
            }

            string doctorName =
                doctorReader.GetString(0);

            string specialization =
                doctorReader.GetString(1);

            int completed =
                doctorReader.GetInt32(2);

            await doctorReader.CloseAsync();


            string historyQuery = @"
                SELECT
                    ch.patient_token,
                    p.name,
                    p.department,
                    ch.consultation_date
                FROM consultation_history ch
                LEFT JOIN patients p
                    ON p.token = ch.patient_token
                WHERE ch.doctor_id = @id
                ORDER BY ch.consultation_date DESC;
            ";

            using var historyCommand =
                new NpgsqlCommand(
                    historyQuery,
                    _connection
                );

            historyCommand.Parameters.AddWithValue(
                "@id",
                doctorId
            );

            await using var reader =
                await historyCommand.ExecuteReaderAsync();

            var history = new List<object>();

            while (await reader.ReadAsync())
            {
                history.Add(new
                {
                    patientToken = reader.GetInt32(0),

                    patientName = reader.IsDBNull(1)
                        ? "Unknown"
                        : reader.GetString(1),

                    department = reader.IsDBNull(2)
                        ? ""
                        : reader.GetString(2),

                    consultationDate = reader.IsDBNull(3)
                        ? (DateTime?)null
                        : reader.GetDateTime(3)
                });
            }

            return Ok(new
            {
                success = true,

                doctor = new
                {
                    doctorId = doctorId,
                    name = doctorName,
                    specialization = specialization,
                    consultationsCompleted = completed
                },

                totalConsultations = history.Count,

                history = history
            });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to fetch doctor history"
            );
        }
        finally
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                await _connection.CloseAsync();
        }
    }
}


// =========================================================
// DOCTOR REQUEST MODEL
// =========================================================

public class DoctorRequest
{
    public int DoctorId { get; set; }

    public string Name { get; set; } = "";

    public string Specialization { get; set; } = "";
}


// =========================================================
// DOCTOR AVAILABILITY REQUEST
// =========================================================

public class DoctorAvailabilityRequest
{
    public bool Available { get; set; }
}
