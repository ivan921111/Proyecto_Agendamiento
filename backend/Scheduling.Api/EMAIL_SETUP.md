# Configuración de Email para Confirmación de Citas

## Descripción
El sistema envía automáticamente un email de confirmación cuando se crea una nueva cita médica.

## Configuración

### 1. Instalar dependencias
El proyecto ya incluye el paquete `MailKit` para envío de emails.

### 2. Configurar credenciales de email
Edita el archivo `appsettings.json` y actualiza la sección `Email`:

```json
{
  "Email": {
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "tu-email@gmail.com",
      "Password": "tu-app-password"
    },
    "From": "tu-email@gmail.com"
  }
}
```

### 3. Configuración para Gmail
Si usas Gmail, necesitas:

1. **Habilitar autenticación de 2 factores** en tu cuenta de Google
2. **Generar una contraseña de aplicación**:
   - Ve a [Google Account Settings](https://myaccount.google.com/)
   - Seguridad → Verificación en 2 pasos → Contraseñas de aplicación
   - Genera una contraseña para "Mail"
   - Usa esta contraseña (sin espacios) en el campo `Password`

### 4. Configuración para otros proveedores
Para otros proveedores de email, actualiza:
- `Host`: Dirección del servidor SMTP
- `Port`: Puerto SMTP (generalmente 587 para TLS, 465 para SSL)
- `Username`: Tu dirección de email completa
- `Password`: Tu contraseña o contraseña de aplicación

### 5. Variables de entorno (opcional)
Para entornos de producción, considera usar variables de entorno:

```bash
export Email__Smtp__Host="smtp.gmail.com"
export Email__Smtp__Port="587"
export Email__Smtp__Username="tu-email@gmail.com"
export Email__Smtp__Password="tu-app-password"
export Email__From="tu-email@gmail.com"
```

## Contenido del Email

El email incluye:
- **Asunto**: "Confirmación de Cita Médica"
- **Destinatario**: Email del paciente
- **Contenido**:
  - Saludo personalizado con nombre del paciente
  - Detalles de la cita: Especialidad, Médico, Fecha y Hora
  - Recordatorio de llegar 15 minutos antes
  - Información sobre cancelación/reprogramación

## Manejo de errores
- Si falla el envío del email, la cita se crea normalmente
- El error se registra en la consola pero no interrumpe el flujo
- Los emails fallidos no afectan la funcionalidad del sistema

## Pruebas
Para probar el envío de emails:
1. Crea una cita desde el frontend
2. Verifica que llegue el email al destinatario
3. Revisa los logs de la consola si hay problemas