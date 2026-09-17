import { useQuery } from "@tanstack/react-query";
import {
  getAppBuildInfo,
  isNewerDevBuild,
  isNewerStableVersion,
  IAppBuildInfo,
  IAppUpdate,
} from "~/helpers/appUpdates";
import { appUpdateQueryKey } from "~/helpers/requests";

import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface IAppUpdateResponse {
  channel: string;
  latestVersion: string;
  url: string;
}

const isAppUpdateResponse = (value: unknown): value is IAppUpdateResponse => {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const response = value as Partial<IAppUpdateResponse>;
  return (
    typeof response.channel === "string" &&
    typeof response.latestVersion === "string" &&
    typeof response.url === "string"
  );
};

const getAppUpdate = async (
  currentBuild: IAppBuildInfo,
  request: ReturnType<typeof useAuth>["request"],
): Promise<IAppUpdate | null> => {
  const response = await request({
    url: "/api/app-update",
    method: "GET",
    params: { channel: currentBuild.channel },
  });

  if (response.status === 204 || !isAppUpdateResponse(response.data)) {
    return null;
  }

  if (response.data.channel !== currentBuild.channel) {
    return null;
  }

  const latestBuild = getAppBuildInfo(response.data.latestVersion);
  if (!latestBuild || latestBuild.channel !== currentBuild.channel) {
    return null;
  }

  const isNewer =
    currentBuild.channel === "stable"
      ? isNewerStableVersion(
          currentBuild.normalizedVersion,
          latestBuild.normalizedVersion,
        )
      : isNewerDevBuild(
          currentBuild.normalizedVersion,
          latestBuild.normalizedVersion,
        );

  if (!isNewer) {
    return null;
  }

  return {
    channel: currentBuild.channel,
    currentVersion: currentBuild.version,
    latestVersion: latestBuild.version,
    url: response.data.url,
  };
};

export const useAppUpdateQuery = () => {
  const { request } = useAuth();
  const currentBuild = getAppBuildInfo(import.meta.env.VITE_VERSION);

  return useQuery<IAppUpdate | null>({
    queryKey: [appUpdateQueryKey, currentBuild?.version ?? "unknown"],
    enabled: currentBuild !== null,
    queryFn: async () => {
      if (!currentBuild) {
        return null;
      }

      try {
        return await getAppUpdate(currentBuild, request);
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
