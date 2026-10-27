using System.Globalization;

namespace SudamericanoNotas
{
    public partial class MainPage : ContentPage
    {
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Opacity = 0;
            TranslationY = 12;

            await Task.WhenAll(
                this.FadeToAsync(1, 550, Easing.CubicOut),
                this.TranslateToAsync(0, 0, 550, Easing.CubicOut)
            );
        }
        public MainPage()
        {
            InitializeComponent();
            Opacity = 0;

            Loaded += async (sender, args) =>
            {
                await this.FadeToAsync(1, 650, Easing.CubicOut);
            };

            // Lista provisional de cinco estudiantes
            EstudiantePicker.ItemsSource = new List<string>
            {
                "Ana López",
                "Carlos Pérez",
                "María Torres",
                "José Ramírez",
                "Daniela Gómez"
            };

            FechaEvaluacion.Date = DateTime.Today;
        }

        // Lee y valida cada calificación
        private async Task<(bool Valida, double Nota)> LeerNotaAsync(
            Entry entrada, string nombre)
        {
            string texto = entrada.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(texto))
            {
                await DisplayAlertAsync(
                    "Campo obligatorio",
                    $"Ingresa la nota de {nombre}.",
                    "Aceptar");

                await EstadoBorder.ScaleToAsync(1.04, 120, Easing.CubicOut);
                await EstadoBorder.ScaleToAsync(1.0, 180, Easing.CubicInOut);

                entrada.Focus();
                return (false, 0);
            }

            // Acepta decimales con coma o punto
            texto = texto.Replace(',', '.');

            bool esNumero = double.TryParse(
                texto,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out double nota);

            if (!esNumero ||
                double.IsNaN(nota) ||
                double.IsInfinity(nota) ||
                nota < 0 ||
                nota > 10)
            {
                await DisplayAlertAsync(
                    "Nota inválida",
                    $"{nombre} debe ser un número entre 0 y 10.",
                    "Aceptar");

                entrada.Focus();
                return (false, 0);
            }

            return (true, nota);
        }

        // Calcula las notas y presenta el reporte
        private async void OnCalcularClicked(
            object sender, EventArgs e)
        {
            Parcial1Label.Text = "— / 5,00";
            Parcial2Label.Text = "— / 5,00";
            NotaFinalLabel.Text = "— / 10";

            EstadoLabel.Text = "Pendiente de cálculo";
            EstadoLabel.TextColor = Color.FromArgb("#6B7890");
            EstadoBorder.BackgroundColor = Color.FromArgb("#EEF1F6");

            if (EstudiantePicker.SelectedIndex < 0)
            {
                await DisplayAlertAsync(
                    "Estudiante obligatorio",
                    "Selecciona uno de los cinco estudiantes.",
                    "Aceptar");

                return;
            }

            var resultadoS1 = await LeerNotaAsync(
                Seguimiento1Entry, "Seguimiento 1");

            if (!resultadoS1.Valida)
                return;

            var resultadoE1 = await LeerNotaAsync(
                Examen1Entry, "Examen 1");

            if (!resultadoE1.Valida)
                return;

            var resultadoS2 = await LeerNotaAsync(
                Seguimiento2Entry, "Seguimiento 2");

            if (!resultadoS2.Valida)
                return;

            var resultadoE2 = await LeerNotaAsync(
                Examen2Entry, "Examen 2");

            if (!resultadoE2.Valida)
                return;

            double s1 = resultadoS1.Nota;
            double e1 = resultadoE1.Nota;
            double s2 = resultadoS2.Nota;
            double e2 = resultadoE2.Nota;

            // Cada parcial suma un máximo de 5 puntos
            double parcial1 = (s1 * 0.3) + (e1 * 0.2);
            double parcial2 = (s2 * 0.3) + (e2 * 0.2);

            double notaFinal = parcial1 + parcial2;

            string estado;
            Color colorEstado;
            Color fondoEstado;

            if (notaFinal >= 7)
            {
                estado = "APROBADO";
                colorEstado = Color.FromArgb("#16865B");
                fondoEstado = Color.FromArgb("#E8F7EF");
            }
            else if (notaFinal >= 5)
            {
                estado = "COMPLEMENTARIO";
                colorEstado = Color.FromArgb("#A96508");
                fondoEstado = Color.FromArgb("#FFF3D9");
            }
            else
            {
                estado = "REPROBADO";
                colorEstado = Color.FromArgb("#C03636");
                fondoEstado = Color.FromArgb("#FDECEC");
            }

            CultureInfo cultura =
                CultureInfo.GetCultureInfo("es-EC");

            string Formatear(double valor)
            {
                return valor.ToString("0.00", cultura);
            }

            // Actualiza las tarjetas de la interfaz
            Parcial1Label.Text =
                $"{Formatear(parcial1)} / 5,00";

            Parcial2Label.Text =
                $"{Formatear(parcial2)} / 5,00";

            NotaFinalLabel.Text =
                $"{Formatear(notaFinal)} / 10";

            EstadoLabel.Text = estado;
            EstadoLabel.TextColor = colorEstado;
            EstadoBorder.BackgroundColor = fondoEstado;

            EstadoBorder.CancelAnimations();
            NotaFinalLabel.CancelAnimations();

            EstadoBorder.Scale = 1;
            NotaFinalLabel.Opacity = 0;

            await Task.WhenAll(
                EstadoBorder.ScaleToAsync(1.04, 160, Easing.CubicOut),
                NotaFinalLabel.FadeToAsync(1, 250, Easing.CubicOut)
            );

            await EstadoBorder.ScaleToAsync(1, 180, Easing.CubicInOut);

            string nombre =
                EstudiantePicker.SelectedItem?.ToString()
                ?? "Sin nombre";

            string fecha = FechaEvaluacion.Date?.ToString("dd/MM/yyyy") ?? "";

            // Reporte solicitado en el reto
            string mensaje =
                $"Nombre: {nombre}\n" +
                $"Fecha: {fecha}\n" +
                $"Nota Parcial 1: {Formatear(parcial1)} / 5,00\n" +
                $"Nota Parcial 2: {Formatear(parcial2)} / 5,00\n" +
                $"Nota Final: {Formatear(notaFinal)} / 10\n" +
                $"Estado: {estado}";

            await DisplayAlertAsync(
                "Reporte de calificaciones",
                mensaje,
                "Aceptar");
        }

        // Limpia todos los campos
        private async Task OnLimpiarClicked(
            object sender, EventArgs e)
        {
            EstudiantePicker.SelectedIndex = -1;
            FechaEvaluacion.Date = DateTime.Today;

            Seguimiento1Entry.Text = string.Empty;
            Examen1Entry.Text = string.Empty;
            Seguimiento2Entry.Text = string.Empty;
            Examen2Entry.Text = string.Empty;

            Parcial1Label.Text = "— / 5,00";
            Parcial2Label.Text = "— / 5,00";
            NotaFinalLabel.Text = "— / 10";

            EstadoLabel.Text = "Pendiente de cálculo";
            EstadoLabel.TextColor =
                Color.FromArgb("#6B7890");

            EstadoBorder.BackgroundColor =
                Color.FromArgb("#EEF1F6");

            EstadoBorder.CancelAnimations();
            await EstadoBorder.ScaleToAsync(0.97, 100, Easing.CubicOut);
            await EstadoBorder.ScaleToAsync(1.0, 180, Easing.CubicInOut);
        }
    }
}
