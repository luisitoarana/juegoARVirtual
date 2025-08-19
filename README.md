Anomalías de la Explosión

"Anomalías de la Explosión" es un juego de realidad aumentada para dispositivos móviles (Android/iOS) desarrollado como proyecto de aprendizaje en la materia de Desarrollo Basado en Plataforma.

El objetivo del juego es explorar un entorno afectado por una misteriosa explosión, encontrar anomalías y descubrir los secretos que se esconden en la historia de la casa. Las anomalías y fantasmas aparecen de forma interactiva según dónde apuntes con la cámara del móvil, generando una experiencia inmersiva y dinámica.

Historia y niveles

El juego se divide en tres niveles:

Fragmentos de la explosión: encuentra 3 anomalías en la sala o cocina.

Sombras del pasado: encuentra 4 anomalías en distintas habitaciones.

El secreto final: encuentra 5 anomalías en toda la casa, incluyendo terraza y baño.

Cada anomalía revela fragmentos de la historia mediante textos y voces, y los fantasmas pueden aparecer aleatoriamente con frases o susurros que aumentan el suspenso.

Cómo jugar

Apunta la cámara de tu dispositivo a superficies planas dentro del espacio físico.

Toca la pantalla para colocar anomalías.

Escucha los sonidos y diálogos asociados a cada anomalía.

Avanza de nivel completando los objetivos de cada etapa.

Requisitos del proyecto

Unity 2021.3 o superior.

Paquetes AR Foundation y ARCore XR Plugin (Android) o ARKit XR Plugin (iOS).

Móvil compatible con AR.

Prefabs de anomalías (pueden ser cubos o planos con texturas de fantasmas).

Audio para efectos de fantasmas y ambiente.

Estructura del proyecto
/Assets
  /Scripts
    - EcosDeLaExplosion.cs
  /Prefabs
    - AnomaliaNivel1.prefab
    - AnomaliaNivel2.prefab
    - AnomaliaNivel3.prefab
  /Audio
    - susurro1.mp3
    - lamento2.mp3
  /UI
    - Canvas
  /Scenes
    - EscenaPrincipal.unity

Instalación y prueba

Abre el proyecto en Unity.

Configura la plataforma a Android/iOS.

Conecta tu dispositivo móvil por USB (modo desarrollador).

Haz Build & Run.

Explora con la cámara, coloca anomalías y disfruta de la experiencia de AR.

Próximos pasos

Agregar animaciones a los fantasmas.

Sistema de reinicio o repetición de niveles.

Integración de voces reales y efectos de susto.

Guardado de progreso del jugador.
