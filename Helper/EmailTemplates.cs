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
        
    }
}