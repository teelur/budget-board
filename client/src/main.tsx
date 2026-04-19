import { StrictMode, Suspense } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import "react-grid-layout/css/styles.css";
import "react-resizable/css/styles.css";
import App from "~/App";
import "~/shared/dayjs.ts";
import "~/i18n/config.ts";
import LoadingScreen from "./components/LoadingScreen/LoadingScreen";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Suspense fallback={<LoadingScreen />}>
      <App />
    </Suspense>
  </StrictMode>,
);
