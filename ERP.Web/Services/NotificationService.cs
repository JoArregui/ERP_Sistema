using System;

namespace ERP.Web.Services
{
    public enum NotificationType 
    { 
        Success, 
        Error, 
        Warning, 
        Info 
    }

    public class NotificationService
    {
        // Evento al que se suscribirá el MainLayout
        public event Action<string, NotificationType>? OnShow;

        /// <summary>
        /// Lanza una notificación visual en la interfaz de usuario.
        /// </summary>
        /// <param name="message">Mensaje a mostrar</param>
        /// <param name="type">Tipo de alerta (Success, Error, etc.)</param>
        public void ShowNotification(string message, NotificationType type)
        {
            OnShow?.Invoke(message, type);
        }
    }
}