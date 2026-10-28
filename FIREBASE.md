# Configuración de Firebase

1. Copia `firebase.example.json` como `firebase.local.json` en la raíz del proyecto, junto al archivo .csproj.
2. Escribe la API key de tu proyecto Firebase en la propiedad `ApiKey`.
3. Compila nuevamente la aplicación. La configuración se incluye como recurso de la aplicación.

`firebase.local.json` está excluido de Git. Comparte la configuración con tus compañeros por un canal privado; no uses `git add -f` para subirla.
Si compilas en CI, crea este archivo antes de compilar a partir de la configuración privada del pipeline.

La aplicación puede compilar sin el archivo, pero las operaciones de Authentication mostrarán un mensaje indicando que falta la configuración.

## Alcance de esta medida

La exclusión evita publicar la clave en nuevos cambios del repositorio, pero el recurso puede extraerse de la aplicación instalada. Una API key de Firebase identifica el proyecto; los permisos sobre los datos deben controlarse mediante Authentication y reglas de Firestore. Restringe la clave a los servicios necesarios en Google Cloud.

Mover la clave no elimina versiones ya publicadas en el historial ni cierra alertas de GitHub. Revisa las restricciones de la clave expuesta y reemplázala si corresponde, actualizando luego el archivo local y recompilando.

Referencia: https://firebase.google.com/docs/projects/api-keys
