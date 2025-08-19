using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class EcosDeLaExplosion : MonoBehaviour
{
    [Header("AR")]
    public ARRaycastManager raycastManager;
    public GameObject[] anomalíasPrefabs;
    public Transform anomalíasParent;

    [Header("UI")]
    public Text textoDialogo;
    public Text textoNivel;
    public GameObject panelDialogo;
    public GameObject botonSiguiente;

    [Header("Audio")]
    public AudioSource audioAmbiental;
    public AudioClip musicaAmbiental;
    public AudioSource audioEfectos;
    public AudioClip[] sonidosFantasma;
    public AudioClip[] frasesAudio;

    [Header("Configuración jugable")]
    public int indicePrefabSustoCercano = 0;
    public float[] tiempoEntreAparicionesPorNivel = new float[] { 30f, 18f, 10f };
    public int[] objetivoPorNivel = new int[] { 3, 4, 12 };

    [Header("Audio (crossfade)")]
    public float crossfadeDuration = 1.6f;
    public float fadeInDuration = 1.0f;

    // Variables internas
    private int nivelActual = 0;
    private int anomalíasMostradas = 0;
    private int anomalíasObjetivo = 0;
    private bool nivelEnCurso = false;
    private bool esperandoInteraccion = false;

    private List<string[]> dialogosPorNivel = new List<string[]>();
    private readonly string[] frasesFantasmaUI = new string[] {
        "¿Me ves?", "No deberías estar aquí...", "Ellos vienen..."
    };
    // Nuevas frases para guiar al jugador
    private readonly string[] frasesDeUbicacion = new string[] {
        "Dirígete al cuarto y toca la pantalla para ver.", // Nivel 1
        "Ahora, ve a la sala y pulsa la pantalla.",        // Nivel 2
        "Dirígete a la cosina  , algo te espera ahí."           // Nivel 3
    };

    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private Coroutine introRoutine = null;
    private Coroutine spawnRoutine = null;
    private Coroutine finalizarRoutine = null;
    private Coroutine ambientRoutine = null;

    private AudioSource audioAmbientalAlt = null;
    private float ambientTargetVolume = 0.5f;

    void Start()
    {
        if (botonSiguiente != null) botonSiguiente.SetActive(false);

        dialogosPorNivel.Add(new string[] { "Esta energía no pertenece a este mundo...", "Algo despertó bajo esta casa...", "Debemos tener cuidado." });
        dialogosPorNivel.Add(new string[] { "Las sombras caminan cuando nadie mira...", "El pasado no se olvida, ni siquiera aquí.", "Escucho susurros, pero no hay nadie." });
        dialogosPorNivel.Add(new string[] { "El tiempo se fracturó...", "La explosión fue un experimento fallido.", "El secreto te costará caro." });

        if (audioAmbiental != null && musicaAmbiental != null)
        {
            ambientTargetVolume = Mathf.Clamp(audioAmbiental.volume, 0f, 1f);
            audioAmbiental.clip = musicaAmbiental;
            audioAmbiental.loop = false;
            audioAmbiental.playOnAwake = false;

            if (audioAmbientalAlt == null)
            {
                GameObject go = new GameObject("AmbientAlt_Auto");
                go.transform.SetParent(transform);
                audioAmbientalAlt = go.AddComponent<AudioSource>();
                audioAmbientalAlt.playOnAwake = false;
                audioAmbientalAlt.loop = false;
                audioAmbientalAlt.clip = musicaAmbiental;
                audioAmbientalAlt.volume = 0f;
            }
            ambientRoutine = StartCoroutine(AmbientLoopCrossfade());
        }

        ComenzarNivel(0);
    }

    // Nuevo método para detectar toques en la pantalla o clicks con el mouse
    void Update()
    {
        if (esperandoInteraccion)
        {
            // Detectar toque en móvil
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ContinuarJuego();
            }

            // Detectar click de mouse en el editor o PC
            if (Input.GetMouseButtonDown(0))
            {
                ContinuarJuego();
            }
        }
    }


    public void ComenzarNivel(int nivel)
    {
        if (nivel < 0) nivel = 0;
        if (nivel >= objetivoPorNivel.Length) nivel = objetivoPorNivel.Length - 1;

        if (introRoutine != null) { StopCoroutine(introRoutine); introRoutine = null; }
        if (spawnRoutine != null) { StopCoroutine(spawnRoutine); spawnRoutine = null; }
        if (finalizarRoutine != null) { StopCoroutine(finalizarRoutine); finalizarRoutine = null; }

        nivelActual = nivel;
        anomalíasMostradas = 0;
        nivelEnCurso = false;
        esperandoInteraccion = false; // Resetear estado
        textoNivel.text = $"Nivel {nivelActual + 1}";
        if (botonSiguiente != null) botonSiguiente.SetActive(false);
        if (panelDialogo != null) panelDialogo.SetActive(true);

        anomalíasObjetivo = objetivoPorNivel[nivel];
        introRoutine = StartCoroutine(IntroduccionNivel());
    }

    IEnumerator IntroduccionNivel()
    {
        string[] frasesIntro = dialogosPorNivel[Mathf.Clamp(nivelActual, 0, dialogosPorNivel.Count - 1)];
        foreach (string frase in frasesIntro)
        {
            MostrarDialogo(frase);
            yield return new WaitForSeconds(2.5f);
        }
        MostrarDialogo(frasesFantasmaUI[Random.Range(0, frasesFantasmaUI.Length)]);
        yield return new WaitForSeconds(1.5f);

        // Nueva lógica de interactividad
        if (panelDialogo != null) panelDialogo.SetActive(true);
        MostrarDialogo(frasesDeUbicacion[nivelActual]);
        esperandoInteraccion = true;
        yield break;
    }

    public void ContinuarJuego()
    {
        if (esperandoInteraccion)
        {
            esperandoInteraccion = false;
            if (panelDialogo != null) panelDialogo.SetActive(false);
            nivelEnCurso = true;
            spawnRoutine = StartCoroutine(SpawnAnomaliasPorNivel());
        }
    }

    IEnumerator SpawnAnomaliasPorNivel()
    {
        float tiempoEntre = tiempoEntreAparicionesPorNivel[Mathf.Clamp(nivelActual, 0, tiempoEntreAparicionesPorNivel.Length - 1)];

        int objetivo = anomalíasObjetivo;
        for (int i = 0; i < objetivo; i++)
        {
            float intervalo = Mathf.Max(0.6f, tiempoEntre * Random.Range(0.2f, 1f));
            yield return new WaitForSeconds(intervalo);
            string comportamiento = ElegirComportamientoAleatorioNivel(nivelActual);
            SpawnAnomaliaConComportamiento(comportamiento);
            anomalíasMostradas++;
        }

        yield return new WaitForSeconds(2f);
        spawnRoutine = null;
        finalizarRoutine = StartCoroutine(FinalizarNivel());
        yield break;
    }

    string ElegirComportamientoAleatorioNivel(int nivel)
    {
        if (nivel == 0)
        {
            string[] pool = new string[] { "parpadear" };
            return pool[Random.Range(0, pool.Length)];
        }
        else if (nivel == 1)
        {
            string[] pool = new string[] { "parpadear", "flotar", "bajar" };
            return pool[Random.Range(0, pool.Length)];
        }
        else
        {
            string[] pool = new string[] { "parpadear", "flotar", "bajar", "moverse" };
            return pool[Random.Range(0, pool.Length)];
        }
    }

    void SpawnAnomaliaConComportamiento(string comportamiento)
    {
        Vector3 spawnPos;

        if (comportamiento == "bajar")
        {
            spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f + Vector3.up * 1.5f;
        }
        else
        {
            spawnPos = ObtenerPuntoDondeApuntas(Camera.main, 2f, 3f);
        }

        int prefabIndex = Random.Range(0, anomalíasPrefabs.Length);
        GameObject go = Instantiate(anomalíasPrefabs[prefabIndex], spawnPos, Quaternion.identity, anomalíasParent);

        StartCoroutine(ManejarComportamientoDeAnomalia(go, comportamiento));

        if (audioEfectos != null && sonidosFantasma.Length > 0)
        {
            audioEfectos.PlayOneShot(sonidosFantasma[Random.Range(0, sonidosFantasma.Length)]);
        }
    }

    IEnumerator ManejarComportamientoDeAnomalia(GameObject anomalia, string comportamiento)
    {
        Renderer anomalíaRenderer = anomalia.GetComponentInChildren<Renderer>();
        Vector3 posicionInicial = anomalia.transform.position;

        if (anomalíaRenderer == null)
        {
            Destroy(anomalia);
            yield break;
        }

        float tiempoDeEntrada = 0.3f;
        Color colorInicial = anomalíaRenderer.material.color;
        anomalíaRenderer.enabled = true;
        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < tiempoDeEntrada)
        {
            float alfa = Mathf.Lerp(0f, colorInicial.a, tiempoTranscurrido / tiempoDeEntrada);
            anomalíaRenderer.material.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alfa);
            anomalia.transform.LookAt(Camera.main.transform.position);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }
        anomalíaRenderer.material.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, colorInicial.a);
        // Modifica  el  timpo  de vida ,que  durara   la  anomalia  en  la  ecena 
        float tiempoDeVida = 0.1f;
        float tiempoRestante = tiempoDeVida;

        while (tiempoRestante > 0)
        {
            anomalia.transform.LookAt(Camera.main.transform.position);

            switch (comportamiento)
            {
                case "parpadear":
                    anomalíaRenderer.enabled = !anomalíaRenderer.enabled;
                    yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
                    break;
                case "flotar":
                    float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * 4f) * 0.1f;
                    anomalia.transform.position = new Vector3(anomalia.transform.position.x, nuevaY, anomalia.transform.position.z);
                    break;
                case "bajar":
                    anomalia.transform.position = Vector3.Lerp(anomalia.transform.position, posicionInicial - Vector3.up * 1f, Time.deltaTime * 5f);
                    break;
                case "moverse":
                    float nuevoX = posicionInicial.x + Mathf.Sin(Time.time * 2.5f) * 0.2f;
                    anomalia.transform.position = new Vector3(nuevoX, anomalia.transform.position.y, anomalia.transform.position.z);
                    break;
                default:
                    yield return null;
                    break;
            }
            tiempoRestante -= Time.deltaTime;
            yield return null;
        }

        float tiempoDeSalida = 0.3f;
        tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < tiempoDeSalida)
        {
            float alfa = Mathf.Lerp(colorInicial.a, 0f, tiempoTranscurrido / tiempoDeSalida);
            anomalíaRenderer.material.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alfa);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        Destroy(anomalia);
    }

    Vector3 ObtenerPuntoDondeApuntas(Camera cam, float minDist, float maxDist)
    {
        Vector3 pos = cam.transform.position + cam.transform.forward * ((minDist + maxDist) * 0.5f);
        if (raycastManager != null)
        {
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (raycastManager.Raycast(screenCenter, s_Hits, TrackableType.Planes))
            {
                var hitPose = s_Hits[0].pose;
                pos = hitPose.position;
            }
        }
        return pos;
    }

    IEnumerator FinalizarNivel()
    {
        nivelEnCurso = false;
        if (panelDialogo != null) panelDialogo.SetActive(true);

        string mensajeFinal = $"Nivel {nivelActual + 1} completado.\nFantasmales encontrados: {anomalíasMostradas}";
        MostrarDialogo(mensajeFinal);
        yield return new WaitForSeconds(3.2f);

        if (botonSiguiente != null) botonSiguiente.SetActive(true);
        finalizarRoutine = null;
    }

    public void BotonSiguienteNivel()
    {
        if (botonSiguiente != null) botonSiguiente.SetActive(false);
        if (nivelActual < objetivoPorNivel.Length - 1)
        {
            ComenzarNivel(nivelActual + 1);
        }
        else
        {
            TerminarJuego();
        }
    }

    void TerminarJuego()
    {
        MostrarDialogo("¡Felicidades! Has completado todos los niveles.");
    }

    void MostrarDialogo(string texto)
    {
        if (textoDialogo != null) textoDialogo.text = texto;
    }

    IEnumerator AmbientLoopCrossfade()
    {
        AudioSource audioActual = audioAmbiental;
        AudioSource audioNuevo = audioAmbientalAlt;
        audioActual.volume = 0f;
        audioNuevo.volume = 0f;
        audioActual.Play();

        while (true)
        {
            yield return StartCoroutine(FadeAudio(audioActual, 0f, ambientTargetVolume, fadeInDuration));
            float espera = audioActual.clip.length - crossfadeDuration;
            yield return new WaitForSeconds(espera);
            audioNuevo.Play();
            yield return StartCoroutine(Crossfade(audioActual, audioNuevo, crossfadeDuration));
            var temp = audioActual;
            audioActual = audioNuevo;
            audioNuevo = temp;
            audioNuevo.Stop();
            audioNuevo.volume = 0f;
        }
    }

    IEnumerator FadeAudio(AudioSource audioSource, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = to;
    }

    IEnumerator Crossfade(AudioSource desde, AudioSource hacia, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            desde.volume = Mathf.Lerp(ambientTargetVolume, 0f, elapsed / duration);
            hacia.volume = Mathf.Lerp(0f, ambientTargetVolume, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        desde.volume = 0f;
        hacia.volume = ambientTargetVolume;
    }
}