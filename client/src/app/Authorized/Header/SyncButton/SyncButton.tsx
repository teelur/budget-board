import { Button } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse, AxiosError } from "axios";
import React from "react";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";

const SyncButton = (): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doSyncMutation = useMutation({
    mutationFn: async () =>
      await request({ url: "/api/simplefin/sync", method: "GET" }),
    onSuccess: async (data: AxiosResponse) => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["goals"] });
      if ((data.data?.length ?? 0) > 0) {
        {
          data.data.map((error: string) =>
            notifications.show({ color: "red", message: error })
          );
        }
      }
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  return (
    <Button
      onClick={() => doSyncMutation.mutate()}
      loading={doSyncMutation.isPending}
    >
      Sync
    </Button>
  );
};

export default SyncButton;
