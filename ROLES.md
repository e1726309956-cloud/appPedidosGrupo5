# Ejemplo de accesos dinámicos

## Configurar Firestore

En la consola de Firebase, crea estos documentos en la colección `roles`.
Los siguientes objetos representan los campos del documento, no el formato REST.
`pantallas` debe ser un mapa y sus valores deben ser booleanos, no textos.

Documento `roles/cliente`:
```json
{"pantallas": {"catalogo": true, "pedidos": false, "administracion": false}}
```

Documento `roles/vendedor`:
```json
{"pantallas": {"catalogo": true, "pedidos": true, "administracion": false}}
```

Documento `roles/administrador`:
```json
{"pantallas": {"catalogo": true, "pedidos": true, "administracion": true}}
```

Esta distribución es únicamente de ejemplo y se puede cambiar. Incluso el administrador
necesita tener la pantalla habilitada para verla en el menú.

En `usuarios/{uid}`, configura `rol` como texto: `cliente`, `vendedor` o `administrador`.
El ID del documento debe coincidir con el UID de Authentication. Usa estos valores
exactos en minúsculas también para que coincidan con las reglas.
El registro existente crea la cuenta de Authentication; el perfil de Firestore todavía
se crea manualmente. El primer administrador debe asignarse desde la consola por un
responsable del proyecto; un usuario no puede elevar su propio rol desde la app.

Revisa las reglas actuales antes de integrar `firestore.rules` en la pestaña Reglas
de Firestore. El archivo de ejemplo deniega las demás colecciones; si ya existen
reglas de negocio, conserva las necesarias e integra las nuevas cuidadosamente.
El archivo local no se publica automáticamente y no se ha cambiado el proyecto remoto.

## Probar

1. Inicia sesión con un usuario de cada rol. Cliente verá Catálogo; vendedor verá
   Catálogo y Pedidos; administrador verá las tres opciones.
2. Cambia `roles/vendedor.pantallas.pedidos` a `false` en la consola.
3. Pulsa Actualizar accesos: Pedidos debe desaparecer. Si pulsas una opción que
   acaba de revocarse, también se vuelve a consultar el permiso antes de abrirla.
4. Prueba un mapa vacío o todos los valores en `false`: aparece el mensaje sin accesos.
5. Un perfil sin rol, rol desconocido, documento ausente o consulta fallida no concede acceso.
6. En el simulador de reglas, comprueba que un cliente no pueda cambiar su rol ni
   editar `roles`, y que solo pueda leer `ejemplos/catalogo` con su permiso habilitado.

## Añadir una pantalla real

1. Crea su página y registra su identificador y nombre en `Pantallas` de `Views/vMenu.cs`.
2. En `AbrirAsync`, reemplaza la página de ejemplo por la página correspondiente
   al identificador, manteniendo la comprobación de permisos antes de navegar.
3. Añade ese identificador al mapa `pantallas` de los tres roles, con `true` o `false`.
   Un identificador ausente no concede acceso.
4. Protege sus colecciones en las reglas de Firestore. El permiso de una pantalla
   no basta para autorizar todas las operaciones: por ejemplo, un cliente solo debe
   leer sus propios pedidos y los permisos de escritura requieren reglas separadas.

Las pantallas del ejemplo no leen ni escriben datos de negocio. La colección
`ejemplos` de las reglas ilustra la validación en el servidor para datos de prueba.
No se necesita crear documentos en esa colección para utilizar el menú.

Los permisos se consultan al iniciar sesión, al regresar al menú, al pulsar Actualizar
accesos y antes de navegar. No hay suscripción en tiempo real: una página que ya está
abierta no se cierra automáticamente al revocar su acceso. Las reglas del servidor
deben proteger cada lectura/escritura. El cliente REST actual no renueva el token;
si vence, hay que volver a iniciar sesión. Cerrar sesión descarta la sesión local.

Referencia: https://firebase.google.com/docs/rules/rules-and-auth
