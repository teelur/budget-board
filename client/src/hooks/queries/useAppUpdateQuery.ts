import { useQuery } from "@tanstack/react-query";
import {
  APP_REPOSITORY,
  getAppBuildInfo,
  isNewerDevBuild,
  isNewerStableVersion,
  IAppBuildInfo,
  IAppUpdate,
} from "~/helpers/appUpdates";
import { appUpdateQueryKey } from "~/helpers/requests";

const GITHUB_API_URL = "https://api.github.com";
const DEV_WORKFLOW_FILE = "docker-image-dev-build.yml";

interface IGitHubReleaseResponse {
  tag_name?: unknown;
  html_url?: unknown;
  draft?: unknown;
  prerelease?: unknown;
}

interface IGitHubWorkflowRun {
  head_sha?: unknown;
  html_url?: unknown;
  conclusion?: unknown;
}

interface IGitHubWorkflowRunsResponse {
  workflow_runs?: unknown;
}

const fetchGitHubJson = async <T>(url: string): Promise<T> => {
  const response = await fetch(url, {
    headers: {
      Accept: "application/vnd.github+json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(`GitHub API returned ${response.status}`);
  }

  return (await response.json()) as T;
};

const getStableUpdate = async (
  currentBuild: IAppBuildInfo,
): Promise<IAppUpdate | null> => {
  const release = await fetchGitHubJson<IGitHubReleaseResponse>(
    `${GITHUB_API_URL}/repos/${APP_REPOSITORY}/releases/latest`,
  );

  if (
    typeof release.tag_name !== "string" ||
    typeof release.html_url !== "string" ||
    release.draft === true ||
    release.prerelease === true
  ) {
    return null;
  }

  const latestBuild = getAppBuildInfo(release.tag_name);
  if (
    !latestBuild ||
    latestBuild.channel !== "stable" ||
    !isNewerStableVersion(
      currentBuild.normalizedVersion,
      latestBuild.normalizedVersion,
    )
  ) {
    return null;
  }

  return {
    channel: "stable",
    currentVersion: currentBuild.version,
    latestVersion: latestBuild.version,
    url: release.html_url,
  };
};

const getDevUpdate = async (
  currentBuild: IAppBuildInfo,
): Promise<IAppUpdate | null> => {
  const workflowRuns = await fetchGitHubJson<IGitHubWorkflowRunsResponse>(
    `${GITHUB_API_URL}/repos/${APP_REPOSITORY}/actions/workflows/${DEV_WORKFLOW_FILE}/runs?branch=main&status=success&per_page=1`,
  );
  const latestRun = Array.isArray(workflowRuns.workflow_runs)
    ? (workflowRuns.workflow_runs[0] as IGitHubWorkflowRun | undefined)
    : undefined;

  if (
    !latestRun ||
    typeof latestRun.head_sha !== "string" ||
    typeof latestRun.html_url !== "string" ||
    latestRun.conclusion !== "success" ||
    !isNewerDevBuild(currentBuild.normalizedVersion, latestRun.head_sha)
  ) {
    return null;
  }

  return {
    channel: "dev",
    currentVersion: currentBuild.version,
    latestVersion: `sha-${latestRun.head_sha.slice(0, 7)}`,
    url: latestRun.html_url,
  };
};

export const useAppUpdateQuery = () => {
  const currentBuild = getAppBuildInfo(import.meta.env.VITE_VERSION);

  return useQuery<IAppUpdate | null>({
    queryKey: [appUpdateQueryKey, currentBuild?.version ?? "unknown"],
    enabled: currentBuild !== null,
    queryFn: async () => {
      if (!currentBuild) {
        return null;
      }

      try {
        return currentBuild.channel === "stable"
          ? await getStableUpdate(currentBuild)
          : await getDevUpdate(currentBuild);
      } catch {
        return null;
      }
    },
    staleTime: Infinity,
    gcTime: Infinity,
    retry: false,
    refetchOnMount: false,
    refetchOnReconnect: false,
    refetchOnWindowFocus: false,
  });
};
