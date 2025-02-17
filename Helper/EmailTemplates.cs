using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Helper
{
    public class EmailTemplates
    {

        [Obsolete("This method is deprecated and will be removed in future versions. Use NewValidationEmail instead.")]
        public static string ValidationEmail(string href){
            return @"<body style='margin:2rem auto;width:36rem'><p>¡Gracias por registrarte en la Fiscalía Digital del Estado de Tamaulipas! Estás a solo un paso de completar la creación de tu cuenta, que será tu <b>llave digital</b> para acceder a todos nuestros servicios en línea.</p><p>Para continuar, necesitamos que valides tu dirección de correo electrónico. Esta validación es importante, ya que te permitirá recibir notificaciones y actualizaciones importantes sobre tus trámites y servicios.</p><p>Por favor, haz clic en el siguiente enlace para confirmar tu correo electrónico y continuar con la captura de tus datos personales:</p><div style='display:flex;justify-items:center;margin-top:2rem'><a href='{urlRef}' target='_blank' style='margin:0 auto;display:flex;align-items:center;justify-content:center;background-color:#627a8b;padding:.25rem 1rem;border:1px solid #566977;border-radius:.25rem;color:white;cursor:pointer;box-shadow:#00000033 0px 2px 4px 1px;text-decoration:none'><span style='margin:0rem 0.5rem;text-transform:uppercase;font-size:1rem'>Continuar</span></a></div><p style='margin-top:2rem'>Estamos comprometidos en ofrecerte un servicio eficiente y seguro. Completa este último paso y empieza a aprovechar todas las ventajas de la Fiscalía Digital.</p><p style='margin-top:2rem'><center>Atentamente</center><b style='padding-top:0.1rem'><center>Fiscalía General de Justicia del Estado de Tamaulipas</center></b></p></body>".Replace("{urlRef}", href);
        }

        public static string PreregisterCode(string code, string time){
            return @"<body><table class='content' width='600' border='0' cellspacing='0' cellpadding='0' style='margin:0 auto;border-collapse:collapse'><tr><td class='body' style='padding:20px 40px;text-align:left;font-size:16px;line-height:1.6'><p>¡Gracias por registrarte en la Fiscalía Digital del Estado de Tamaulipas! Estás a solo un paso de completar la creación de tu cuenta, que será tu <b>llave digital</b> para acceder a todos nuestros servicios en línea.</p><p>Para completar el proceso, por favor utilice el siguiente código de verificación:</p></td></tr><tr><td style='padding:10px 20px'><div style='margin:0 auto;padding:10px 20px 10px 45px;background-color:#345c72;border-radius:5px;width:fit-content;text-align:center;font-size:1.75rem;color:#ffffff;text-decoration:none;font-weight:bold;letter-spacing:1.5rem;font-family:consolas,monospace'>{{code}}</div></td></tr><tr><td class='body' style='padding:10px 40px;text-align:left;font-size:16px;line-height:1.6'><p>Este código es válido por un tiempo limitado y deberá utilizarlo antes de las {{time}}. Si no realiza el restablecimiento antes de esta hora, tendrá que solicitar un nuevo código.</p></td></tr><tr><td class='body' style='padding:10px 40px;text-align:left;font-size:16px;line-height:1.6'><b>Importante: </b><ul><li>No comparta este código con nadie más.</li><li>El personal de la Fiscalía nunca le pedirá su contraseña ni este código de verificación.</li></ul></td></tr><tr><td class='body' style='padding:10px 40px;text-align:left;font-size:16px;line-height:1.6'><p style='margin-top:2rem'>Estamos comprometidos en ofrecerte un servicio eficiente y seguro. Completa este último paso y empieza a aprovechar todas las ventajas de la Fiscalía Digital.</p></td></tr></table><div style='margin-top:2rem;text-align:center'>Atentamente</div><center>Fiscalía General de Justicia del Estado de Tamaulipas</center></body>".Replace("{{code}}", code).Replace("{{time}}", time);
        }

        public static string ResetPassword(string href){
            return "<body><table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td align='center' style='padding: 20px;'><table class='content' width='600' border='0' cellspacing='0' cellpadding='0' style='border-collapse: collapse;'><tr><td class='body' style='padding: 40px; text-align: left; font-size: 16px; line-height: 1.6;'>Para restablecer su contraseña, siga el enlace siguiente:</td></tr><tr><td style='padding: 0px 40px 0px 40px; text-align: center;'><table cellspacing='0' cellpadding='0' style='margin: auto;'><tr><td align='center' style='background-color: #345C72; padding: 10px 20px; border-radius: 5px;'><a href='{{urlRef}}' target='_blank' style='color: #ffffff; text-decoration: none; font-weight: bold;'>Restablecer contraseña</a></td></tr></table></td></tr><tr><td class='body' style='padding: 40px; text-align: left; font-size: 16px; line-height: 1.6;'>Si tiene alguna pregunta o necesita más ayuda, no dude en ponerse en contacto con nuestro equipo de asistencia.</td></tr></table></td></tr></table></body>".Replace("{{urlRef}}", href);
        }

        public static string ResetPasswordCode(string code, string time){
            return @"<body>
                <table class='content' width='600' border='0' cellspacing='0' cellpadding='0' style='margin:0 auto;border-collapse:collapse;'>
                    <tr><td class='body' style='padding:20px 40px;text-align:left;font-size:16px;line-height:1.6;'>
                        <p>Hemos recibido su solicitud para restablecer la contraseña de su llave digital. Para completar el proceso, por favor utilice el siguiente código de verificación:</p>
                    </td></tr>
                    <tr><td style='padding:10px 20px;'>
                        <div style='margin:0 auto;padding:10px 20px 10px 45px;background-color:#345C72;border-radius:5px;width:fit-content;text-align:center;font-size:1.75rem;color:#ffffff;text-decoration:none;font-weight:bold;letter-spacing:1.5rem;font-family:consolas,monospace;'>{{code}}</div>
                    </td></tr>
                    <tr><td class='body' style='padding:10px 40px;text-align:left;font-size:16px;line-height:1.6;'>
                        <p>Este código es válido por un tiempo limitado y deberá utilizarlo antes de las {{time}}. Si no realiza el restablecimiento antes de esta hora, tendrá que solicitar un nuevo código.</p>
                    </td></tr>
                    <tr><td class='body' style='padding:10px 40px;text-align:left;font-size:16px;line-height:1.6;'>
                        <b>Importante: </b>
                        <ul>
                            <li>No comparta este código con nadie más.</li>
                            <li>El personal de la Fiscalía nunca le pedirá su contraseña ni este código de verificación.</li>
                        </ul>
                    </td></tr>
                    <tr><td class='body' style='padding:10px 40px;text-align:left;font-size:16px;line-height:1.6;'>
                        <p>Si usted no solicitó el restablecimiento de su contraseña, puede ignorar este correo electrónico. Su cuenta permanecerá segura y sin cambios.</p>
                    </td></tr>
                </table>
                <div style='margin-top:2rem;text-align:center;'>Atentamente</div>
                <b style='padding-top:0.2rem;text-align:center;'>Fiscalía General de Justicia del Estado de Tamaulipas</b> </body>".Replace("{{code}}", code).Replace("{{time}}", time);
        }

        public static string Welcome(string personFullName, string imageNameSrc, string imageProfileSrc, string welcomeMessage){
            // welcomeMessage = "¡Bienvenido(a) a la Fiscalía Digital!"
            return @"<body style='margin:2rem auto;width:40rem'><h2 style='text-align:center'>{welcome-message}<br/>{user-name}</h2><p>Nos complace informarle que su cuenta recién creada es mucho más que un simple registro, es su <b>llave digital</b> para acceder a una amplia gama de servicios ofrecidos por la Fiscalía General de Justicia del Estado de Tamaulipas.</p><p>Con esta llave digital, podrá:</p><ul><li><b>Presentar denuncias en línea</b> de manera rápida y segura.</li><li><b>Obtener constancias de antecedentes penales</b> sin tener que desplazarse.</li><li><b>Reportar el extravío de documentos</b> desde cualquier lugar.</li><li><b>Localizar oficinas</b> de la Fiscalía con facilidad.</li><li><b>Presentar quejas en línea</b> contra servidores públicos de la Fiscalía.</li></ul><p>Su cuenta le permite gestionar todos estos servicios desde un solo lugar, brindándole la comodidad de acceder a la justicia y a la información que necesita, cuando la necesita.</p><p style='margin-top:1rem'>Puede actualizar sus datos personales en cualquier momento haciendo clic en su nombre, asegurándose de que su llave digital siempre esté al día.</p><img style='width:36rem;margin:.25rem auto;background:#627a8b;padding:.25rem;border:1px solid #566977;border-radius:.25rem;box-shadow:#00000033 0px 2px 12px 1px' src='{image-name}' alt='imagen descriptiva opcion perfil'/><p style='margin-top:2rem'>Además, tiene acceso a un apartado donde podrá consultar el historial de todos sus trámites.</p><img style='width:36rem;margin:.25rem auto;background:#627a8b;padding:.25rem;border:1px solid #566977;border-radius:.25rem;box-shadow:#00000033 0px 2px 12px 1px' src='{image-profile}' alt='imagen descriptiva opcion consulta tramites'/><p style='margin-top:2rem'>Nuestro compromiso es brindarle un servicio eficiente y transparente, asegurando que sus derechos sean protegidos y que la justicia esté al alcance de todos.</p><p><b>Gracias por confiar en nosotros.</b></p><div style='display:flex;justify-items:center;margin-top:2rem'><a href='https://fiscaliadigital.fgjtam.gob.mx/mi-perfil' style='margin:0 auto;display:flex;align-items:center;justify-content:center;background-color:#627a8b;padding:.25rem 1rem;border:1px solid #566977;border-radius:.25rem;color:white;cursor:pointer;box-shadow:#00000033 0px 2px 4px 1px;text-decoration:none'><span style='text-transform:uppercase;font-size:1rem'>Ver mi perfil</span></a></div><p style='margin-top:2rem'><center>Atentamente</center><b style='padding-top:.1rem'><center>Fiscalía General de Justicia del Estado de Tamaulipas</center></b></p></body>"
                .Replace("{user-name}", personFullName)
                .Replace("{image-name}", imageNameSrc)
                .Replace("{image-profile}", imageProfileSrc)
                .Replace("{welcome-message}", welcomeMessage);
        }

        public static string CodeChangeEmail(string code, string time){
            return @"<body>
            <table style='margin:0 auto;border-collapse:collapse;' class='content' width='600' border='0' cellspacing='0' cellpadding='0'>
                <tr><td class='body' style='padding:10px 40px;text-align:left;font-size:16px;line-height:1.6;'>
                    <p>Hemos recibido una solicitud para cambiar la dirección de correo electrónico asociada a su cuenta. Si usted ha solicitado este cambio, por favor, utilice el siguiente código de verificación para confirmar la actualización de su correo electrónico:</p>
                </td></tr>
                <tr><td style='padding:10px 20px;'>
                    <div style='margin:0 auto;padding:10px 20px 10px 45px;background-color:#345C72;border-radius:5px;width:fit-content;text-align:center;font-size:1.75rem;color:#ffffff;text-decoration:none;font-weight:bold;letter-spacing:1.5rem;font-family:consolas,monospace;'>{code}</div>
                </td></tr>
                <tr><td class='body' style='padding:10px 40px;text-align:left;font-size:16px;line-height:1.6;'>
                    <p>Si no ha solicitado un cambio de correo electrónico, ignore este mensaje. Por su seguridad, este código caducará a las {time}.</p>
                </td></tr>
                <tr><td class='body' style='padding:0px 40px;text-align:left;font-size:16px;line-height:1.6;'>
                    <b>Importante: </b>
                    <ul>
                        <li>No comparta este código con nadie más.</li>
                        <li>El personal de la Fiscalía nunca le pedirá su contraseña ni este código de verificación.</li>
                    </ul>
                </td></tr>
            </table>
            <div style='margin-top:2rem;text-align:center;'>Atentamente</div>
            <b style='padding-top:0.2rem;text-align:center;'>Fiscalía General de Justicia del Estado de Tamaulipas</b>
            </body>".Replace("{code}", code).Replace("{time}", time);
        }

        public static string RecoveryAccountCompleted(string personFullName, string email){
            return @"
            <body>
            <table style='margin:0 auto;border-collapse:collapse;' class='content' width='600' border='0' cellspacing='0' cellpadding='0'>
            <tr>
            <td class='body' style='padding:2px 40px;text-align:left;font-size:16px;line-height:1.6;'>
            <p>Estimado/a <b>{personName}</b></p>
            <p>Hemos recibido y procesado su solicitud para restablecer su cuenta en la Fiscalía Digital.</p>
            <p>Tras validar los datos proporcionados (CURP, nombre, fecha de nacimiento, entre otros), hemos actualizado el correo electrónico asociado a su llave digital en el sistema.</p>
            </td>
            </tr>
            <tr>
            <td class='body' style='padding:2px 40px;text-align:left;font-size:16px;line-height:1.6;'>
            <b>Próximo paso:</b>
            <p>Para completar el proceso y garantizar la seguridad de su cuenta, es necesario que restablezca su contraseña. Por favor, siga estos pasos:
            <ol>
            <li>Acceda al portal de Fiscalía Digital en fiscaliadigital.fgjtam.gob.mx.</li>
            <li>Seleccione la opción 'Restablecer contraseña' fiscaliadigital.fgjtam.gob.mx/restablecer-cuenta.</li>
            <li>Ingrese el nuevo correo electrónico asociado a su cuenta: <b>{email}</b>, recibirá un código de verificación en su correo electrónico.</li>
            <li>Introduzca el código de verificación en el portal junto con su nueva contraseña.</li>
            </ol>
            <p>Una vez realizado este proceso, podrá iniciar sesión utilizando su nuevo correo electrónico y la contraseña que haya establecido.</p>
            <p>Gracias por su confianza y colaboración.</p>
            </td>
            </tr>
            <tr>
            <td class='body' style='padding:2px 40px;text-align:left;font-size:16px;line-height:1.6;'>
            <div style='margin-top:2rem;text-align:center;'>Atentamente</div>
            <div style='padding-top:0.2rem;text-align:center;font-weight:bold;'>Fiscalía General de Justicia del Estado de Tamaulipas</div>
            </td>
            </tr>
            </table>
            </body>".Replace("{personName}", personFullName).Replace("{email}", email);
        }

        public static string RecoveryAccountInCompleted(string personFullName, string comments){
            return @"
            <body>
            <table style='margin:0 auto;border-collapse:collapse;' class='content' width='600' border='0' cellspacing='0' cellpadding='0'>
            <tr>
            <td class='body' style='padding:2px 40px;text-align:left;font-size:16px;line-height:1.6;'>
            <p>Estimado/a <b>{personName}</b></p>
            <p>Hemos recibido su solicitud para restablecer su cuenta en la Fiscalía Digital. Sin embargo, tras revisar los datos proporcionados, no hemos podido confirmar que la cuenta le pertenece debido a la insuficiencia de información.</p>
            <p>{comments}</p>
            <p>Para garantizar la protección de sus datos y proceder con la recuperación de su cuenta, le pedimos que genere una nueva solicitud siguiendo estas indicaciones:</p>
            </td>
            </tr>
            <tr>
            <td class='body' style='padding:2px 40px;text-align:left;font-size:16px;line-height:1.6;'>
            <b>Pasos a seguir:</b>
            <ol>
            <li>Acceda al portal de Fiscalía Digital en <a href='https://fiscaliadigital.fgjtam.gob.mx/recuperar-cuenta'>https://fiscaliadigital.fgjtam.gob.mx/recuperar-cuenta</a></li>
            <li>Complete el formulario de recuperación de cuenta, asegurándose de ingresar toda su información personal (CURP, nombre completo, fecha de nacimiento, etc.).</li>
            <li>Anexe una fotografía o un documento PDF de su identificación oficial (INE, pasaporte, o cualquier documento oficial válido).</li>
            </ol>
            <p>Entre más datos proporcione, mejor podremos asegurar la protección de su información y agilizar el proceso de recuperación.</p>
            <p>Agradecemos su comprensión y colaboración.</p>
            </td>
            </tr>
            <tr>
            <td class='body' style='padding:2px 40px;text-align:left;font-size:16px;line-height:1.6;'>
            <div style='margin-top:2rem;text-align:center;'>Atentamente</div>
            <div style='padding-top:0.2rem;text-align:center;font-weight:bold;'>Fiscalía General de Justicia del Estado de Tamaulipas</div>
            </td>
            </tr>
            </table>
            </body>".Replace("{personName}", personFullName).Replace("{comments}", comments);
        }

    }
}