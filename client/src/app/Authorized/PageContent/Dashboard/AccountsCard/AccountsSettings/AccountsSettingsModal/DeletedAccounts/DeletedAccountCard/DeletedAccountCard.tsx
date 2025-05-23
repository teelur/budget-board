import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import classes from "./DeletedAccountCard.module.css";

import { getDaysSinceDate } from "~/helpers/datetime";
import { Button, Card, Group, Text } from "@mantine/core";
import { IAccount } from "~/models/account";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { AxiosError } from "axios";

interface DeletedAccountCardProps {
  deletedAccount: IAccount;
}

const DeletedAccountCard = (
  props: DeletedAccountCardProps
): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doRestoreAccount = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: "/api/account/restore",
        method: "POST",
        params: { guid: id },
      }),
    onSuccess: async () => {
      // Refetch the accounts and institutions queries immediatly after the account is restored
      await queryClient.refetchQueries({ queryKey: ["institutions"] });
      await queryClient.refetchQueries({ queryKey: ["accounts"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  return (
    <Card className={classes.card} shadow="xs" radius="lg">
      <Text className={classes.column}>{props.deletedAccount.name}</Text>
      <Text className={classes.column} size="sm">
        {`${getDaysSinceDate(
          props.deletedAccount.deleted!
        )} days since deleted`}
      </Text>
      <Group className={classes.button}>
        <Button
          loading={doRestoreAccount.isPending}
          onClick={() => doRestoreAccount.mutate(props.deletedAccount.id)}
        >
          <Undo2Icon size={16} />
        </Button>
      </Group>
    </Card>
  );
};

export default DeletedAccountCard;
