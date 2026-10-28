namespace appPedidosGrupo5;

/// <summary>Mensajes compartidos de validación, errores y alertas.</summary>
public static class Constantes
{
    public const string ErrorConfiguracionAusente = "Falta la configuración de Firebase. Copia firebase.example.json a firebase.local.json, configura ApiKey y vuelve a compilar la aplicación.";
    public const string ErrorPerfilAusente = "No se encontró el perfil en usuarios/{0}. Comprueba que el ID del documento coincida con el UID de Authentication.";
    public const string ErrorRolAusente = "No existe el documento roles/{0}. Créalo en Firestore con el campo pantallas de tipo mapa.";
    public const string ErrorPermisosDenegados = "Firestore denegó roles/{0}. Publica firestore.rules y verifica que usuarios/{1} tenga un campo rol válido.";
    public const string ErrorConsultaPermisos = "No se pudieron consultar los permisos. Revisa el documento roles/{0} y las reglas de Firestore.";
    public const string ErrorConexion = "No se pudo conectar. Revisa tu conexión a Internet.";
    public const string ErrorTiempoEspera = "La solicitud tardó demasiado. Revisa tu conexión e inténtalo nuevamente.";
    public const string ErrorDatosFirebase = "Firebase respondió con datos inválidos. Inténtalo nuevamente.";
    public const string ErrorFormatoFirebase = "La respuesta de Firebase no contiene datos válidos.";
    public const string ErrorGeneral = "No se pudo completar la operación. Inténtalo nuevamente.";
    public const string ErrorIdentificadorCuenta = "Firebase no devolvió el identificador de la cuenta creada.";
    public const string ErrorIdentificadorSesion = "Firebase no devolvió el identificador de la sesión.";
    public const string ErrorSesionInvalida = "Firebase no devolvió una sesión válida.";
    public const string ErrorTokenInvalido = "Firebase devolvió un token de sesión inválido.";
    public const string ErrorProyectoFirebase = "No se pudo identificar el proyecto de Firebase.";
    public const string ErrorLecturaRol = "Firestore no permitió consultar el rol. Revisa las reglas de lectura del perfil del usuario.";
    public const string ErrorSesionRechazada = "Firestore no aceptó la sesión. Vuelve a iniciar sesión.";
    public const string ErrorConsultaRol = "No se pudo consultar el rol en Firestore. Inténtalo nuevamente.";
    public const string ErrorRolInvalido = "El perfil existe, pero el campo rol debe contener un texto válido.";
    public const string ErrorContrasenaFirebase = "La contraseña no cumple los requisitos de Firebase.";
    public const string ErrorCorreoRegistrado = "Ya existe una cuenta con ese correo.";
    public const string ErrorCorreoFirebase = "El correo electrónico no es válido.";
    public const string ErrorCredenciales = "El correo o la contraseña son incorrectos.";
    public const string ErrorCuentaDeshabilitada = "Esta cuenta está deshabilitada.";
    public const string ErrorDemasiadosIntentos = "Demasiados intentos. Inténtalo más tarde.";
    public const string ErrorProveedorDeshabilitado = "Activa correo y contraseña en Firebase Authentication.";
    public const string ErrorOperacionFirebase = "No se pudo completar la operación con Firebase.";
    public const string ErrorRolDesconocido = "El usuario no tiene un rol reconocido: cliente, administrador o vendedor.";
    public const string ErrorSesionVencida = "La sesión venció. Cierra sesión e ingresa nuevamente.";
    public const string ErrorMapaPantallas = "El documento del rol debe tener un campo pantallas de tipo mapa.";
    public const string ErrorConfiguracionJson = "firebase.local.json no contiene un JSON válido. Corrige el archivo y vuelve a compilar.";
    public const string ErrorApiKey = "Configura ApiKey en firebase.local.json y vuelve a compilar la aplicación.";
    public const string CorreoRequerido = "Ingresa un correo electrónico válido.";
    public const string CredencialesRequeridas = "Ingresa un correo válido y tu contraseña.";
    public const string ContrasenaCorta = "Usa al menos 6 caracteres.";
    public const string ContrasenasDistintas = "Las contraseñas no coinciden.";
    public const string SinInternet = "No tienes acceso a Internet. Conéctate a una red e inténtalo nuevamente.";
    public const string ErrorCargarAccesos = "No se pudieron cargar los accesos.";
    public const string AccesosCambiaron = "Tus accesos cambiaron. Pulsa Actualizar accesos.";
    public const string AccesoDenegado = "Tu rol no tiene acceso a esta pantalla.";
    public const string ReintentarAccesos = "Actualiza los accesos o vuelve a iniciar sesión.";
    public const string TituloRevisarDatos = "Revisa tus datos";
    public const string TituloRevisarContrasena = "Revisa tu contraseña";
    public const string TituloSinInternet = "Sin conexión a Internet";
    public const string TituloErrorRegistro = "No se pudo crear la cuenta";
    public const string TituloErrorRecuperacion = "No se pudo enviar el enlace";
    public const string TituloErrorAccesos = "No se pudieron cargar los accesos";
    public const string TituloErrorLogin = "No se pudo iniciar sesión";
    public const string TituloAccesos = "Accesos";
    public const string TituloAccesoRestringido = "Acceso restringido";
    public const string Aceptar = "Aceptar";
}
