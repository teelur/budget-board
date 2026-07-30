import fs from "node:fs";
import path from "node:path";

const root = process.cwd();
const enPath = path.join(root, "public/locales/en-us/translation.json");
const ptPath = path.join(root, "public/locales/pt-br/translation.json");

const readJson = (filePath) => JSON.parse(fs.readFileSync(filePath, "utf8"));

const en = readJson(enPath);
const pt = readJson(ptPath);

const sortedKeys = (value) => Object.keys(value).sort();
const enKeys = sortedKeys(en);
const ptKeys = sortedKeys(pt);

const fail = (message) => {
  console.error(message);
  process.exitCode = 1;
};

const missing = enKeys.filter((key) => !ptKeys.includes(key));
const extra = ptKeys.filter((key) => !enKeys.includes(key));

if (missing.length > 0) {
  fail(`Missing PT-BR keys: ${missing.join(", ")}`);
}

if (extra.length > 0) {
  fail(`Extra PT-BR keys: ${extra.join(", ")}`);
}

const uniqueMatches = (value, regex) => {
  const matches = String(value).match(regex) ?? [];
  return [...new Set(matches)].sort();
};

const interpolationPattern = /\{\{[^}]+\}\}/g;
const transTagPattern = /<\/?\d+>/g;

for (const key of enKeys) {
  const enInterpolations = uniqueMatches(en[key], interpolationPattern);
  const ptInterpolations = uniqueMatches(pt[key], interpolationPattern);
  const enTransTags = uniqueMatches(en[key], transTagPattern);
  const ptTransTags = uniqueMatches(pt[key], transTagPattern);

  if (enInterpolations.join("|") !== ptInterpolations.join("|")) {
    fail(
      `Interpolation mismatch for ${key}: en=[${enInterpolations.join(", ")}] pt=[${ptInterpolations.join(", ")}]`,
    );
  }

  if (enTransTags.join("|") !== ptTransTags.join("|")) {
    fail(
      `Trans tag mismatch for ${key}: en=[${enTransTags.join(", ")}] pt=[${ptTransTags.join(", ")}]`,
    );
  }
}

if (!process.exitCode) {
  console.log(`PT-BR translation matches ${enKeys.length} English keys.`);
}
