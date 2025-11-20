namespace ContractMonthlyClaimSystem_starter.ViewModels
{
    public class ClaimCreateViewModel
    {
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public string Notes { get; set; }
    }
}
@using ContractMonthlyClaimSystem_starter.ViewModels
@model ClaimCreateViewModel
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}
< div class= "center-card" >
    < h1 > Submit Claim </ h1 >
    < p class= "subtext" > Lecturers can submit claims quickly</p>
    <form asp-action="Create" method="post" enctype="multipart/form-data">
        <label>Hours Worked</label>
        <input asp-for="HoursWorked" type="number" step="0.25" class= "input" />
        < label > Hourly Rate </ label >
        < input asp -for= "HourlyRate" type = "number" step = "0.01" class= "input" />
        < label > Notes </ label >
        < textarea asp -for= "Notes" class= "input" rows = "4" ></ textarea >
        < label > Upload supporting docs(.pdf, .docx, .xlsx) -max 5MB each</label>
        <input type = "file" name= "Files" multiple class= "input" />
        < button class= "btn-primary" type = "submit" > Submit </ button >
    </ form >
</ div >
