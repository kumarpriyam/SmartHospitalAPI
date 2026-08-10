using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace SmartHospitalAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public DashboardController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        await _connection.OpenAsync();

        var patientCommand = new NpgsqlCommand(
            @"SELECT
                COUNT(*) AS total,
                COUNT(*) FILTER (WHERE status = 'WAITING'),
                COUNT(*) FILTER (WHERE status = 'WITH DOCTOR'),
                COUNT(*) FILTER (WHERE status = 'COMPLETED')
              FROM patients",
            _connection);

        await using var patientReader =
            await patientCommand.ExecuteReaderAsync();

        await patientReader.ReadAsync();

        int totalPatients = Convert.ToInt32(
            patientReader.GetInt64(0));

        int waiting = Convert.ToInt32(
            patientReader.GetInt64(1));

        int withDoctor = Convert.ToInt32(
            patientReader.GetInt64(2));

        int completed = Convert.ToInt32(
            patientReader.GetInt64(3));

        await patientReader.CloseAsync();

        var doctorCommand = new NpgsqlCommand(
            @"SELECT
                COUNT(*),
                COUNT(*) FILTER (WHERE available = TRUE),
                COUNT(*) FILTER (WHERE available = FALSE)
              FROM doctors",
            _connection);

        await using var doctorReader =
            await doctorCommand.ExecuteReaderAsync();

        await doctorReader.ReadAsync();

        int totalDoctors = Convert.ToInt32(
            doctorReader.GetInt64(0));

        int availableDoctors = Convert.ToInt32(
            doctorReader.GetInt64(1));

        int busyDoctors = Convert.ToInt32(
            doctorReader.GetInt64(2));

        return Ok(new
        {
            success = true,

            patients = new
            {
                total = totalPatients,
                waiting,
                withDoctor,
                completed
            },

            doctors = new
            {
                total = totalDoctors,
                available = availableDoctors,
                busy = busyDoctors
            }
        });
    }
}

