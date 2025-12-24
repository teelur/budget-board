// Core i18next library.
import i18n from "i18next";
// Bindings for React: allow components to
// re-render when language changes.
import { initReactI18next } from "react-i18next";
// Backend to load translations from files.
import HttpBackend from "i18next-http-backend";

i18n
  // Add React bindings as a plugin.
  .use(initReactI18next)
  // Add backend to load translations from files.
  .use(HttpBackend)
  // Initialize the i18next instance.
  .init({
    // Config options

    // Fallback locale used when a translation is
    // missing in the active locale.
    fallbackLng: "en-us",

    // Enables useful output in the browser’s
    // dev console.
    debug: true,

    // Load fallback language
    load: "all",

    // Convert case to lowercase for consistency
    lowerCaseLng: true,

    // Normally, we want `escapeValue: true` as it
    // ensures that i18next escapes any code in
    // translation messages, safeguarding against
    // XSS (cross-site scripting) attacks. However,
    // React does this escaping itself, so we turn
    // it off in i18next.
    interpolation: {
      escapeValue: false,
    },

    // Backend configuration to load translations from files.
    backend: {
      // Path to load translations from.
      // {{lng}} will be replaced with the language code.
      // {{ns}} will be replaced with the namespace (default is 'translation').
      loadPath: "/locales/{{lng}}/{{ns}}.json",
    },
  });

export default i18n;
