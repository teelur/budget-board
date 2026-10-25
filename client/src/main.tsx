import { StrictMode, Suspense } from "react";
import { createRoot } from "react-dom/client";
import "@teelur/budget-board-ui/styles.css";
import "@fontsource-variable/ibm-plex-sans/index.css";
import "@fontsource-variable/plus-jakarta-sans/index.css";
import "./index.css";
import "react-grid-layout/css/styles.css";
import "react-resizable/css/styles.css";
import App from "~/App";
import "~/shared/dayjs.ts";
import "~/i18n/config.ts";
import LoadingScreen from "./components/LoadingScreen/LoadingScreen";

const root = document.getElementById("root")!;

createRoot(root).render(
  <StrictMode>
    <Suspense
      fallback={<LoadingScreen message={root.dataset.startupMessage} />}
    >
      <App />
    </Suspense>
  </StrictMode>,
);
