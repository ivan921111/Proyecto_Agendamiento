using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Scheduling.Api.Application.Dtos.Citas;

namespace Scheduling.Api.Application.Services;

public interface IPdfService
{
    byte[] GenerateAppointmentPdf(string patientName, string doctorName, string specialty, DateTime date, TimeSpan time);
    byte[] GenerateMedicAppointmentReportPdf(string medicoNombre, string medicoApellido, string especialidad, List<ReporteCitasDto> citas);
}

public class PdfService : IPdfService
{
    public byte[] GenerateAppointmentPdf(string patientName, string doctorName, string specialty, DateTime date, TimeSpan time)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Header().Text("Confirmación de Cita Médica").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Text($"Paciente: {patientName}").FontSize(14);
                    column.Item().Text($"Médico: {doctorName}").FontSize(14);
                    column.Item().Text($"Especialidad: {specialty}").FontSize(14);
                    column.Item().Text($"Fecha: {date:dd/MM/yyyy}").FontSize(14);
                    column.Item().Text($"Hora: {time.ToString("hh\\:mm")}").FontSize(14);

                    column.Item().PaddingVertical(10).Text("Por favor, llega 15 minutos antes de tu cita.\n\nGracias por utilizar el sistema de agendamiento.").FontSize(12);
                });

                page.Footer().AlignCenter().Text($"Generado el {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC").FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateMedicAppointmentReportPdf(string medicoNombre, string medicoApellido, string especialidad, List<ReporteCitasDto> citas)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Header().Text($"Reporte de Citas - Dr(a). {medicoNombre} {medicoApellido}").FontSize(22).SemiBold().FontColor(Colors.Blue.Medium);

                page.Content().Column(column =>
                {
                    column.Spacing(5);

                    column.Item().Text($"Especialidad: {especialidad}").FontSize(12);
                    column.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(11).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingVertical(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);

                    // Tabla de citas
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Encabezado de tabla
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Medium).Padding(8).Text("Fecha").FontColor(Colors.White).SemiBold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(8).Text("Hora").FontColor(Colors.White).SemiBold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(8).Text("Paciente").FontColor(Colors.White).SemiBold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(8).Text("Estado").FontColor(Colors.White).SemiBold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(8).Text("Email Paciente").FontColor(Colors.White).SemiBold();
                        });

                        // Filas de datos
                        foreach (var cita in citas)
                        {
                            var estadoColor = cita.Estado switch
                            {
                                "Pendiente" => Colors.Orange.Medium,
                                "Completada" => Colors.Green.Medium,
                                "Cancelada" => Colors.Red.Medium,
                                _ => Colors.Grey.Medium
                            };

                            table.Cell().Padding(8).Text(cita.FechaCita.ToString("dd/MM/yyyy")).FontSize(11);
                            table.Cell().Padding(8).Text(cita.HoraCita.ToString(@"hh\:mm")).FontSize(11);
                            table.Cell().Padding(8).Text(cita.PacienteNombre).FontSize(11);
                            table.Cell().Padding(8).Background(estadoColor).Text(cita.Estado).FontColor(Colors.White).SemiBold().FontSize(10);
                            table.Cell().Padding(8).Text(cita.PacienteEmail ?? "-").FontSize(10);
                        }
                    });

                    column.Item().PaddingVertical(10).Text($"Total de citas: {citas.Count}").SemiBold().FontSize(12);
                });

                page.Footer().AlignCenter().Text($"Documento generado automáticamente el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }
}
