using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace SmartHospitalAPI.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public ReportsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }


    // =========================================================
    // DAILY HOSPITAL REPORT
    // GET: /api/reports/daily
    // =========================================================
    [HttpGet("daily")]
    public async Task<IActionResult> DailyReport()
    {
        try
        {
            await _connection.OpenAsync();

            // =================================================
            // PATIENT REPORT
            // =================================================
            string patientQuery = @"
                SELECT
                    COUNT(*) AS total,

                    COUNT(*) FILTER (
                        WHERE status = 'WAITING'
                    ) AS waiting,

                    COUNT(*) FILTER (
                        WHERE status = 'WITH DOCTOR'
                    ) AS with_doctor,

                    COUNT(*) FILTER (
                        WHERE status = 'COMPLETED'
                    ) AS completed,

                    COUNT(*) FILTER (
                        WHERE type = 'Emergency'
                           OR type = 'EMERGENCY'
                           OR type = 'EMERGENCY OVERRIDE'
                    ) AS emergency,

                    COUNT(*) FILTER (
                        WHERE has_appointment = TRUE
                          AND appointment_cancelled = FALSE
                    ) AS active_appointments,

                    COUNT(*) FILTER (
                        WHERE appointment_cancelled = TRUE
                    ) AS cancelled_appointments

                FROM patients;
            ";

            await using var patientCommand =
                new NpgsqlCommand(
                    patientQuery,
                    _connection
                );

            await using var reader =
                await patientCommand.ExecuteReaderAsync();

            await reader.ReadAsync();

            int total =
                Convert.ToInt32(reader.GetInt64(0));

            int waiting =
                Convert.ToInt32(reader.GetInt64(1));

            int withDoctor =
                Convert.ToInt32(reader.GetInt64(2));

            int completed =
                Convert.ToInt32(reader.GetInt64(3));

            int emergency =
                Convert.ToInt32(reader.GetInt64(4));

            int appointments =
                Convert.ToInt32(reader.GetInt64(5));

            int cancelledAppointments =
                Convert.ToInt32(reader.GetInt64(6));

            await reader.CloseAsync();


            // =================================================
            // DOCTOR REPORT
            // =================================================
            string doctorQuery = @"
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

            await using var doctorCommand =
                new NpgsqlCommand(
                    doctorQuery,
                    _connection
                );

            await using var doctorReader =
                await doctorCommand.ExecuteReaderAsync();

            var doctors = new List<object>();

            int availableDoctors = 0;
            int busyDoctors = 0;
            int totalConsultations = 0;

            while (await doctorReader.ReadAsync())
            {
                bool available =
                    doctorReader.GetBoolean(3);

                int consultations =
                    doctorReader.GetInt32(4);

                if (available)
                    availableDoctors++;
                else
                    busyDoctors++;

                totalConsultations += consultations;

                doctors.Add(new
                {
                    doctorId =
                        doctorReader.GetInt32(0),

                    name =
                        doctorReader.GetString(1),

                    specialization =
                        doctorReader.GetString(2),

                    available =
                        available,

                    status =
                        available
                            ? "AVAILABLE"
                            : "BUSY",

                    consultationsCompleted =
                        consultations,

                    currentPatientToken =
                        doctorReader.IsDBNull(5)
                            ? -1
                            : doctorReader.GetInt32(5)
                });
            }

            return Ok(new
            {
                success = true,

                reportDate =
                    DateTime.Now.ToString("yyyy-MM-dd"),

                patients = new
                {
                    total = total,
                    waiting = waiting,
                    withDoctor = withDoctor,
                    completed = completed,
                    emergency = emergency
                },

                appointments = new
                {
                    total = appointments,
                    cancelled = cancelledAppointments
                },

                doctors = new
                {
                    total = doctors.Count,
                    available = availableDoctors,
                    busy = busyDoctors,
                    totalConsultations = totalConsultations,
                    list = doctors
                },

                doctorList = doctors
            });
        }
        catch (PostgresException ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Database error while generating report"
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to generate hospital report"
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
    // SIMPLE REPORT SUMMARY
    // GET: /api/reports/summary
    // =========================================================
    [HttpGet("summary")]
    public async Task<IActionResult> ReportSummary()
    {
        try
        {
            await _connection.OpenAsync();

            string query = @"
                SELECT

                    (SELECT COUNT(*)
                     FROM patients) AS total_patients,

                    (SELECT COUNT(*)
                     FROM patients
                     WHERE status = 'WAITING') AS waiting_patients,

                    (SELECT COUNT(*)
                     FROM patients
                     WHERE status = 'WITH DOCTOR') AS with_doctor,

                    (SELECT COUNT(*)
                     FROM patients
                     WHERE status = 'COMPLETED') AS completed_patients,

                    (SELECT COUNT(*)
                     FROM doctors) AS total_doctors,

                    (SELECT COUNT(*)
                     FROM doctors
                     WHERE available = TRUE) AS available_doctors,

                    (SELECT COUNT(*)
                     FROM doctors
                     WHERE available = FALSE) AS busy_doctors,

                    (SELECT COUNT(*)
                     FROM patients
                     WHERE has_appointment = TRUE
                       AND appointment_cancelled = FALSE)
                    AS active_appointments,

                    (SELECT COUNT(*)
                     FROM patients
                     WHERE appointment_cancelled = TRUE)
                    AS cancelled_appointments;
            ";

            await using var command =
                new NpgsqlCommand(query, _connection);

            await using var reader =
                await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            return Ok(new
            {
                success = true,

                patients = new
                {
                    total =
                        Convert.ToInt32(
                            reader.GetInt64(0)
                        ),

                    waiting =
                        Convert.ToInt32(
                            reader.GetInt64(1)
                        ),

                    withDoctor =
                        Convert.ToInt32(
                            reader.GetInt64(2)
                        ),

                    completed =
                        Convert.ToInt32(
                            reader.GetInt64(3)
                        )
                },

                doctors = new
                {
                    total =
                        Convert.ToInt32(
                            reader.GetInt64(4)
                        ),

                    available =
                        Convert.ToInt32(
                            reader.GetInt64(5)
                        ),

                    busy =
                        Convert.ToInt32(
                            reader.GetInt64(6)
                        )
                },

                appointments = new
                {
                    active =
                        Convert.ToInt32(
                            reader.GetInt64(7)
                        ),

                    cancelled =
                        Convert.ToInt32(
                            reader.GetInt64(8)
                        )
                }
            });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Failed to generate report summary"
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

