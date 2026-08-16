import { notifications } from "@mantine/notifications";
import { useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { ITransactionImportRequest } from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface IImportTransactionsMutationVariables {
  importedTransactions: ITransactionImportRequest;
  idempotencyKey: string;
}

export const useImportTransactionsMutation = () => {
  const { request } = useAuth();

  return useMutation({
    mutationFn: async ({
      importedTransactions,
      idempotencyKey,
    }: IImportTransactionsMutationVariables) =>
      await request({
        url: "/api/transaction/import",
        method: "POST",
        data: importedTransactions,
        headers: {
          "Idempotency-Key": idempotencyKey,
        },
      }),
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
