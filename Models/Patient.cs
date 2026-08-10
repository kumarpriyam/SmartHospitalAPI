namespace SmartHospitalAPI.Models
{
    public class Patient
    {
        public int Token { get; set; }

        public string Name { get; set; } = "";

        public string Gender { get; set; } = "";

        public int Age { get; set; }

        public string Location { get; set; } = "";

        public int Priority { get; set; }

        public string Type { get; set; } = "";

        public string Department { get; set; } = "";

        public string Status { get; set; } = "WAITING";

        public int AssignedDoctorId { get; set; } = -1;

        public string AppointmentDate { get; set; } = "";

        public string AppointmentTime { get; set; } = "";

        public bool HasAppointment { get; set; }

        public bool AppointmentCancelled { get; set; }
    }
}