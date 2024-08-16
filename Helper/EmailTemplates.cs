using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Helper
{
    public class EmailTemplates
    {

        public static string ValidationEmail(string href){
            return "<body><table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td align='center' style='padding: 20px;'><table class='content' width='600' border='0' cellspacing='0' cellpadding='0' style='border-collapse: collapse;'><tr><td class='body' style='padding: 40px; text-align: left; font-size: 16px; line-height: 1.6;'>Gracias por darse de alta, para continuar con el proceso de registro necesitamos verificar su dirección de correo electrónico para garantizar la seguridad de su cuenta.<br/><br/>Haga clic en el siguiente enlace</td></tr><tr><td style='padding: 0px 40px 0px 40px; text-align: center;'><table cellspacing='0' cellpadding='0' style='margin: auto;'><tr><td align='center' style='background-color: #345C72; padding: 10px 20px; border-radius: 5px;'><a href='{{urlRef}}' target='_blank' style='color: #ffffff; text-decoration: none; font-weight: bold;'>Continuar</a></td></tr></table></td></tr><tr><td class='body' style='padding: 40px; text-align: left; font-size: 16px; line-height: 1.6;'>Una vez que el enlace se abra en su navegador, será dirigido a una página que confirma la verificación de su correo electrónico y continuará con la captura de sus datos generales.<br/><br/> Si no se ha registrado o tiene alguna duda sobre este correo electrónico, no lo tenga en cuenta. Su seguridad es nuestra máxima prioridad y tomamos todas las medidas necesarias para proteger su información</td></tr></table></td></tr></table></body>".Replace("{{urlRef}}", href);
        }

        public static string ResetPassword(string href){
            return "<body><table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td align='center' style='padding: 20px;'><table class='content' width='600' border='0' cellspacing='0' cellpadding='0' style='border-collapse: collapse;'><tr><td class='body' style='padding: 40px; text-align: left; font-size: 16px; line-height: 1.6;'>Para restablecer su contraseña, siga el enlace siguiente:</td></tr><tr><td style='padding: 0px 40px 0px 40px; text-align: center;'><table cellspacing='0' cellpadding='0' style='margin: auto;'><tr><td align='center' style='background-color: #345C72; padding: 10px 20px; border-radius: 5px;'><a href='{{urlRef}}' target='_blank' style='color: #ffffff; text-decoration: none; font-weight: bold;'>Restablecer contraseña</a></td></tr></table></td></tr><tr><td class='body' style='padding: 40px; text-align: left; font-size: 16px; line-height: 1.6;'>Si tiene alguna pregunta o necesita más ayuda, no dude en ponerse en contacto con nuestro equipo de asistencia.</td></tr></table></td></tr></table></body>".Replace("{{urlRef}}", href);
        }
        public static string ResetPasswordCode(string code, string time){
            return "<body><table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td align='center' style='padding:20px;'><table class='content' width='600' border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse;'><tr><td class='body' style='padding:40px; text-align:left; font-size:16px; line-height:1.6;'>Hemos recibido una solicitud para restablecer la contraseña de su cuenta. Utilice el siguiente código para restablecer su contraseña:</td></tr><tr><td style='padding:0 40px; text-align:center;'><table cellspacing='0' cellpadding='0' style='margin:auto;'><tr><td align='center' style='background-color:#345C72; padding:10px 20px; border-radius:5px;'><div style='font-size:1.75rem; color:#ffffff; text-decoration:none; font-weight:bold; letter-spacing:1.5rem;font-family:'consolas',monospace;'>{{code}}</div></td></tr></table></td></tr><tr><td class='body' style='padding:40px; text-align:left; font-size:16px; line-height:1.6;'>Si no ha solicitado un restablecimiento de contraseña, ignore este correo electrónico. Este enlace caducará a las {{time}} por su seguridad.<br/><br/>Si tiene alguna pregunta o necesita más ayuda, no dude en ponerse en contacto con nuestro equipo de asistencia.</td></tr></table></td></tr></table></body>".Replace("{{code}}", code).Replace("{{time}}", time);;
        }

        public static string Welcome(string personFullName, string imageNameSrc, string imageProfileSrc, string welcomeMessage){
            // welcomeMessage = "¡Bienvenido(a) a la Fiscalía Digital!"
            return @"<body style='
                    margin: 2rem auto 2rem auto;
                    width: 40rem;
                '>

                <h2 style='text-align: center;'>
                    {welcome-message}
                    <br/>
                    {user-name}
                </h2>

                <p>Con su cuenta, tiene acceso completo a todos los servicios digitales que ofrece la Fiscalía General de Justicia del Estado de Tamaulipas, de manera fácil y rápida. Aquí tiene algunas de las opciones disponibles:</p>
                <ul>
                    <li>Denuncia en línea</li>
                    <li>Constancia de antecedentes penales</li>
                    <li>Extravio de documentos</li>
                    <li>Ubicación de oficinas</li>
                    <li>Queja en línea contra servidores públicos de la Fiscalía`</li>
                </ul>
                
                <p style='margin-top: 2rem;'>
                    Puede actualizar sus datos personales en cualquier momento haciendo clic en su nombre
                </p>
                <img style='
                        width: 36rem;
                        margin: .25rem auto;
                        background: #ddd;
                        padding: .25rem;
                        border: solid 1px #bbb;
                        border-radius: .25rem;
                        box-shadow: #3333334b 2px 0px 12px;
                    '
                    src='{image-name}'
                    alt='imagen descriptiva opcion consulta tramites'
                />
                
                <p style='margin-top: 2rem;'>
                    Además,tiene acceso a un apartado donde podrá consultar el historial de todos sus trámites.
                </p>
                <img style='
                        width: 36rem;
                        margin: .25rem auto;
                        background: #ddd;
                        padding: .25rem;
                        border: solid 1px #bbb;
                        border-radius: .25rem;
                        box-shadow: #3333334b 2px 0px 12px;
                    '
                    src='{image-profile}'
                    alt='imagen descriptiva opcion consulta tramites'
                />
                
                <p style='margin-top: 2rem;'>
                    Nuestro compromiso es brindarle un servicio eficiente y transparente, asegurando que sus derechos sean protegidos y que la justicia esté al alcance de todos.
                </p>
                <p>Gracias por confiar en nosotros.</p>
                
                <p style='margin-top: 4rem;'>
                    <center>Atentamente</center>
                    <b style='padding-top:0.1rem;'><center>Fiscalía General de Justicia del Estado de Tamaulipas</center></b>
                </p>
            </body>".Replace("{user-name}", personFullName)
                .Replace("{image-name}", imageNameSrc)
                .Replace("{image-profile}", imageProfileSrc)
                .Replace("{welcome-message}", welcomeMessage);
        }
        
    }
}