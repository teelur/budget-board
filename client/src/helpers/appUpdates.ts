export const APP_REPOSITORY = "teelur/budget-board";

export type AppBuildChannel = "stable" | "dev";

export interface IAppBuildInfo {
  channel: AppBuildChannel;
  version: string;
  normalizedVersion: string;
}

export interface IAppUpdate {
  channel: AppBuildChannel;
  currentVersion: string;
  latestVersion: string;
  url: string;
}

const stableVersionPattern = /^v?(\d+)\.(\d+)\.(\d+)$/i;
const devVersionPattern = /^sha-([0-9a-f]{7,40})$/i;

const compareNumericVersions = (left: string, right: string): number => {
  const leftParts = left.split(".").map(Number);
  const rightParts = right.split(".").map(Number);

  for (let index = 0; index < 3; index += 1) {
    const leftPart = leftParts[index] ?? 0;
    const rightPart = rightParts[index] ?? 0;

    if (leftPart !== rightPart) {
      return leftPart > rightPart ? 1 : -1;
    }
  }

  return 0;
};

export const getAppBuildInfo = (
  version: string | undefined,
): IAppBuildInfo | null => {
  if (!version) {
    return null;
  }

  const stableMatch = version.trim().match(stableVersionPattern);
  if (stableMatch) {
    return {
      channel: "stable",
      version: version.trim(),
      normalizedVersion: stableMatch.slice(1).join("."),
    };
  }

  const devMatch = version.trim().match(devVersionPattern);
  if (devMatch) {
    return {
      channel: "dev",
      version: version.trim(),
      normalizedVersion: devMatch[1]?.toLowerCase() ?? "",
    };
  }

  return null;
};

export const isNewerStableVersion = (
  currentVersion: string,
  latestVersion: string,
): boolean => compareNumericVersions(currentVersion, latestVersion) < 0;

export const isNewerDevBuild = (
  currentCommit: string,
  latestCommit: string,
): boolean =>
  !latestCommit.toLowerCase().startsWith(currentCommit.toLowerCase());
