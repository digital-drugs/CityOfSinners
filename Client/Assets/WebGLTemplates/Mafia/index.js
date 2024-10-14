window.addEventListener("load", function () {
  if ("serviceWorker" in navigator) {
    navigator.serviceWorker.register("ServiceWorker.js");
  }
});
var unityInstanceRef;
var unsubscribe;
var container = document.querySelector("#unity-container");
var canvas = document.querySelector("#unity-canvas");
var loadingBar = document.querySelector("#unity-loading-bar");
var progressBarFull = document.querySelector("#unity-progress-bar-full");
var warningBanner = document.querySelector("#unity-warning");

// Shows a temporary message banner/ribbon for a few seconds, or
// a permanent error message on top of the canvas if type=='error'.
// If type=='warning', a yellow highlight color is used.
// Modify or remove this function to customize the visually presented
// way that non-critical warnings and error messages are presented to the
// user.
function unityShowBanner(msg, type) {
  function updateBannerVisibility() {
    warningBanner.style.display = warningBanner.children.length ? 'block' : 'none';
  }
  var div = document.createElement('div');
  div.innerHTML = msg;
  warningBanner.appendChild(div);
  if (type == 'error') div.style = 'background: red; padding: 10px;';
  else {
    if (type == 'warning') div.style = 'background: yellow; padding: 10px;';
    setTimeout(function () {
      warningBanner.removeChild(div);
      updateBannerVisibility();
    }, 5000);
  }
  updateBannerVisibility();
}

var buildUrl = "Build";
var loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}";
var config = {
  dataUrl: buildUrl + "/{{{ DATA_FILENAME }}}",
  frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
  #if USE_THREADS
  workerUrl: buildUrl + "/{{{ WORKER_FILENAME }}}",
  #endif
#if USE_WASM
  codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",
  #endif
#if MEMORY_FILENAME
  memoryUrl: buildUrl + "/{{{ MEMORY_FILENAME }}}",
  #endif
#if SYMBOLS_FILENAME
  symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
  #endif
  streamingAssetsUrl: "StreamingAssets",
  companyName: {{{ JSON.stringify(COMPANY_NAME) }}},
productName: { { { JSON.stringify(PRODUCT_NAME) } } },
productVersion: { { { JSON.stringify(PRODUCT_VERSION) } } },
showBanner: unityShowBanner,
};

// By default Unity keeps WebGL canvas render target size matched with
// the DOM size of the canvas element (scaled by window.devicePixelRatio)
// Set this to false if you want to decouple this synchronization from
// happening inside the engine, and you would instead like to size up
// the canvas DOM size and WebGL render target sizes yourself.
// config.matchWebGLToCanvasSize = false;

if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
  // Mobile device style: fill the whole browser client area with the game canvas:
  var meta = document.createElement('meta');
  meta.name = 'viewport';
  meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
  document.getElementsByTagName('head')[0].appendChild(meta);
}

#if BACKGROUND_FILENAME
canvas.style.background = "url('" + buildUrl + "/{{{ BACKGROUND_FILENAME.replace(/'/g, '%27') }}}') center / cover";
#endif
loadingBar.style.display = "block";

var script = document.createElement("script");
script.src = loaderUrl;
script.onload = () => {
  createUnityInstance(canvas, config, (progress) => {
    progressBarFull.style.width = 100 * progress + "%";
  }).then((unityInstance) => {
    unityInstanceRef = unityInstance;
    loadingBar.style.display = "none";
  }).catch((message) => {
    alert(message);
  });
};
document.body.appendChild(script);

function startVKAuth() {
  if (typeof VKIDSDK !== 'undefined') {
    const VKID = window.VKIDSDK;
    VKID.Config.init({
      app: 52308537, // Замените на ваш ID приложения
      redirectUrl: 'https://cityofsinners.ru',
      state: 'jhkerfkjKJfduif_fdsjkwee-2j32krvilasfnHHdfb3i39945hkm_ymnc-23jn1_dj__nj3i1n63bfsjJWI77Byn3',
      mode: VKID.ConfigAuthMode.InNewWindow
    });

    const floatingOneTap = new VKID.FloatingOneTap();
    floatingOneTap.render({
      appName: 'City Of Game',
      onSuccess: function () {
        console.log('Аутентификация прошла успешно');
      },
      onFailure: function (error) {
        console.error('Ошибка аутентификации:', error);
      }
    });

    renderOneTapButton(VKID);
  } else {
    console.error('VKIDSDK не загружен');
  }
}

function renderOneTapButton(VKID) {
  // Создание экземпляра кнопки.
  const oneTap = new VKID.OneTap();

  // Получение контейнера из разметки.
  const container = document.getElementById('VkIdSdkOneTap');
  
  // Проверка наличия кнопки в разметке.
  if (container) {
    // Отрисовка кнопки в контейнере с именем приложения APP_NAME, светлой темой и на русском языке.
    oneTap.render({ container: container, scheme: VKID.Scheme.LIGHT, lang: VKID.Languages.RUS })
        .on(VKID.WidgetEvents.ERROR, console.log); // handleError — какой-либо обработчик ошибки.
  }
}