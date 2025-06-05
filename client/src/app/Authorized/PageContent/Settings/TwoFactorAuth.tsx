import classes from "./Settings.module.css";

import {
  Badge,
  Card,
  CardSection,
  Group,
  LoadingOverlay,
  Title,
  Text,
  Code,
  Flex,
  TextInput,
  Button,
} from "@mantine/core";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import { AxiosError, AxiosResponse } from "axios";
import { useField } from "@mantine/form";

type TwoFactorAuthResponse = {
  sharedKey: string;
  recoveryCodesLeft: number;
  isTwoFactorEnabled: boolean;
  isMachineRemembered: boolean;
};

type TwoFactorAuthRequest = {
  enable?: boolean;
  twoFactorCode?: string;
  resetSharedKey: boolean;
  resetRecoveryCodes: boolean;
  forgetMachine: boolean;
};

const TwoFactorAuth = (): React.ReactNode => {
  const validationCodeField = useField<string>({
    initialValue: "",
    validate: (value) => {
      if (!value) {
        return "Validation code is required";
      }
      return null;
    },
  });

  const { request } = React.useContext<any>(AuthContext);

  const twoFactorAuthQuery = useQuery({
    queryKey: ["twoFactorAuth"],
    queryFn: async (): Promise<TwoFactorAuthResponse | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/manage/2fa",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as TwoFactorAuthResponse;
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doSetTwoFactorAuth = useMutation({
    mutationFn: async (twoFactorAuthData: TwoFactorAuthRequest) =>
      await request({
        url: "/api/manage/2fa",
        method: "POST",
        data: { ...twoFactorAuthData },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
      notifications.show({
        color: "green",
        message: "2FA successfully .",
      });
    },
    onError: (error: AxiosError) => {
      if (error?.response?.data) {
        const errorData = error.response.data as ValidationError;
        if (
          error.status === 400 &&
          errorData.title === "One or more validation errors occurred."
        ) {
          notifications.show({
            title: "One or more validation errors occurred.",
            color: "red",
            message: Object.values(errorData.errors).join("\n"),
          });
        }
      } else {
        notifications.show({
          color: "red",
          message: translateAxiosError(error),
        });
      }
    },
  });

  return (
    <Card className={classes.card} withBorder radius="md" shadow="sm">
      <CardSection>
        <Group>
          <Title order={3}>Two Factor Authentication</Title>
          {twoFactorAuthQuery.data?.isTwoFactorEnabled ? (
            <Badge color="green" maw={80}>
              Enabled
            </Badge>
          ) : (
            <Badge color="red" maw={80}>
              Disabled
            </Badge>
          )}
        </Group>
      </CardSection>
      <CardSection className={classes.cardSection}>
        <LoadingOverlay visible={doSetTwoFactorAuth.isPending} />
        <Flex gap="0.5rem" align="center">
          <Text>Enter this code into your authenticator app:</Text>
          {/* TODO: Format this */}
          <Code>{twoFactorAuthQuery.data?.sharedKey}</Code>
        </Flex>
        <TextInput
          placeholder="Validation Code"
          {...validationCodeField.getInputProps()}
        />
        <Button
          onClick={() =>
            doSetTwoFactorAuth.mutate({
              enable: true,
              twoFactorCode: validationCodeField.getValue(),
              resetSharedKey: false,
              resetRecoveryCodes: false,
              forgetMachine: false,
            } as TwoFactorAuthRequest)
          }
        >
          Enable 2FA
        </Button>
      </CardSection>
    </Card>
  );
};

export default TwoFactorAuth;
