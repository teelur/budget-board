import { StrictMode, Suspense } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "~/App";
import "~/shared/dayjs.ts";
import "~/i18n/config.ts";
import SuspenseLoadingScreen from "./components/SuspenseLoadingScreen/SuspenseLoadingScreen";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Suspense fallback={<SuspenseLoadingScreen />}>
      <App />
    </Suspense>
  </StrictMode>
);
