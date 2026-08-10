using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace SmartHospitalAPI.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public AppointmentController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // =========================================================
    // SCHEDULE APPOINTMENT
    // POST: /api/appointments/1002
    // =========================================================
    [HttpPost("{token:int}")]
    public async Task<IActionResult> ScheduleAppointment(
        int token,
        [FromBody] AppointmentRequest request)
    {
        try
        {
            if (token <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid patient token."
                });
            }

            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Appointment data is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Date))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Appointment date is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Time))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Appointment time is required."
                });
            }

            await _connection.OpenAsync();

            // -------------------------------------------------
            // CHECK PATIENT
            // -------------------------------------------------
            string checkQuery = @"
                SELECT
                    name,
                    status
                FROM patients
                WHERE token = @token;
            ";

            await using var checkCommand =
                new NpgsqlCommand(checkQuery, _connection);

            checkCommand.Parameters.AddWithValue(
                "@token",
                token
            );

            await using var reader =
                await checkCommand.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient not found."
                });
            }

            string patientName = reader.GetString(0);
            string status = reader.GetString(1);

            await reader.CloseAsync();

            // -------------------------------------------------
            // COMPLETED PATIENT CHECK
            // -------------------------------------------------
            if (status.Equals(
                "COMPLETED",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Completed patient cannot be scheduled."
                });
            }

            // -------------------------------------------------
            // SCHEDULE APPOINTMENT
            // -------------------------------------------------
            string updateQuery = @"
                UPDATE patients
                SET
                    appointment_date = @date,
                    appointment_time = @time,
                    has_appointment = TRUE,
                    appointment_cancelled = FALSE
                WHERE token = @token;
            ";

            await using var updateCommand =
                new NpgsqlCommand(updateQuery, _connection);

            updateCommand.Parameters.AddWithValue(
                "@date",
                request.Date.Trim()
            );

            updateCommand.Parameters.AddWithValue(
                "@time",
                request.Time.Trim()
            );

            updateCommand.Parameters.AddWithValue(
                "@token",
                token
            );

            int rows =
                await updateCommand.ExecuteNonQueryAsync();

            if (rows == 0)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Appointment scheduled successfully!",

                appointment = new
                {
                    token = token,
                    patientName = patientName,
                    date = request.Date.Trim(),
                    time = request.Time.Trim(),
                    status = "SCHEDULED"
                }
            });
        }
        catch (PostgresException ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Database error while scheduling appointment"
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to schedule appointment"
            );
        }
        finally
        {
            if (_connection.State ==
                System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }
    }


    // =========================================================
    // CANCEL APPOINTMENT
    // DELETE: /api/appointments/1002
    // =========================================================
    [HttpDelete("{token:int}")]
    public async Task<IActionResult> CancelAppointment(int token)
    {
        try
        {
            if (token <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid patient token."
                });
            }

            await _connection.OpenAsync();

            string query = @"
                UPDATE patients
                SET
                    has_appointment = FALSE,
                    appointment_cancelled = TRUE,
                    appointment_date = '',
                    appointment_time = ''
                WHERE token = @token
                  AND has_appointment = TRUE
                  AND appointment_cancelled = FALSE
                RETURNING name;
            ";

            await using var command =
                new NpgsqlCommand(query, _connection);

            command.Parameters.AddWithValue(
                "@token",
                token
            );

            object? result =
                await command.ExecuteScalarAsync();

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Active appointment not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Appointment cancelled successfully!",
                token = token,
                patientName = result.ToString()
            });
        }
        catch (PostgresException ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Database error while cancelling appointment"
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to cancel appointment"
            );
        }
        finally
        {
            if (_connection.State ==
                System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }
    }


    // =========================================================
    // GET ALL ACTIVE APPOINTMENTS
    // GET: /api/appointments
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        try
        {
            await _connection.OpenAsync();

            string query = @"
                SELECT
                    token,
                    name,
                    age,
                    department,
                    appointment_date::text,
                    appointment_time::text,
                    status
                FROM patients
                WHERE has_appointment = TRUE
                  AND appointment_cancelled = FALSE
                ORDER BY
                    appointment_date,
                    appointment_time;
            ";

            await using var command =
                new NpgsqlCommand(query, _connection);

            await using var reader =
                await command.ExecuteReaderAsync();

            var appointments = new List<object>();

            while (await reader.ReadAsync())
            {
                appointments.Add(new
                {
                    token = reader.GetInt32(0),

                    name = reader.IsDBNull(1)
                        ? ""
                        : reader.GetString(1),

                    age = reader.IsDBNull(2)
                        ? 0
                        : reader.GetInt32(2),

                    department = reader.IsDBNull(3)
                        ? ""
                        : reader.GetString(3),

                    appointmentDate =
                        reader.IsDBNull(4)
                            ? ""
                            : reader.GetString(4),

                    appointmentTime =
                        reader.IsDBNull(5)
                            ? ""
                            : reader.GetString(5),

                    status = reader.IsDBNull(6)
                        ? ""
                        : reader.GetString(6)
                });
            }

            return Ok(new
            {
                success = true,
                totalAppointments = appointments.Count,
                appointments = appointments
            });
        }
        catch (PostgresException ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Database error while fetching appointments"
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to fetch appointments"
            );
        }
        finally
        {
            if (_connection.State ==
                System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }
    }


    // =========================================================
    // GET APPOINTMENT BY PATIENT TOKEN
    // GET: /api/appointments/patient/1002
    // =========================================================
    [HttpGet("patient/{token:int}")]
    public async Task<IActionResult> GetPatientAppointment(int token)
    {
        try
        {
            await _connection.OpenAsync();

            string query = @"
                SELECT
                    token,
                    name,
                    department,
                    appointment_date::text,
                    appointment_time::text,
                    has_appointment,
                    appointment_cancelled,
                    status
                FROM patients
                WHERE token = @token;
            ";

            await using var command =
                new NpgsqlCommand(query, _connection);

            command.Parameters.AddWithValue(
                "@token",
                token
            );

            await using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient not found."
                });
            }

            bool hasAppointment =
                reader.GetBoolean(5);

            bool cancelled =
                reader.GetBoolean(6);

            return Ok(new
            {
                success = true,

                appointment = new
                {
                    token = reader.GetInt32(0),
                    patientName = reader.GetString(1),
                    department = reader.GetString(2),

                    date = reader.IsDBNull(3)
                        ? ""
                        : reader.GetString(3),

                    time = reader.IsDBNull(4)
                        ? ""
                        : reader.GetString(4),

                    hasAppointment = hasAppointment,
                    cancelled = cancelled,
                    status = reader.GetString(7)
                }
            });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to fetch patient appointment"
            );
        }
        finally
        {
            if (_connection.State ==
                System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }
    }
}


// =========================================================
// APPOINTMENT REQUEST MODEL
// =========================================================
public class AppointmentRequest
{
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
}
