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
  Button,
  Stack,
  PinInput,
  CopyButton,
  Skeleton,
} from "@mantine/core";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import { AxiosError, AxiosResponse } from "axios";
import { useField } from "@mantine/form";
import { QRCodeSVG } from "qrcode.react";
import { useDisclosure } from "@mantine/hooks";

type TwoFactorAuthResponse = {
  sharedKey: string;
  recoveryCodesLeft: number;
  recoveryCodes: string[] | null;
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
  const [recoveryCodes, setRecoveryCodes] = React.useState<string[]>([]);
  const [showAuthenticatorSetup, { toggle }] = useDisclosure();

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
    onSuccess: async (res: AxiosResponse) => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
      await queryClient.invalidateQueries({ queryKey: ["twoFactorAuth"] });

      const data = res.data as TwoFactorAuthResponse;
      if (!data) {
        notifications.show({
          color: "red",
          message: "No data returned from the server.",
        });
        return;
      }

      notifications.show({
        color: "green",
        message: "2FA successfully updated.",
      });
      if (data.recoveryCodes) {
        setRecoveryCodes(data.recoveryCodes);
      } else {
        setRecoveryCodes([]);
      }
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

  const formatKey = (key: string): string => {
    // Format the shared key into groups of 4 characters
    return key
      .replace(/(.{4})/g, "$1 ")
      .trim()
      .toLowerCase();
  };

  const buildAuthenticatorUrl = (sharedKey: string): string =>
    `otpauth://totp/Budget%20Board?secret=${sharedKey}`;

  const getAuthenticatorCardContent = (): React.ReactNode => {
    if (twoFactorAuthQuery.data?.isTwoFactorEnabled) {
      return (
        <Stack gap="1rem">
          {recoveryCodes.length > 0 && (
            <Stack gap="0.5rem" align="center">
              <Text size="md" fw={600}>
                Recovery Codes
              </Text>
              <Text size="sm" c="dimmed">
                Keep these codes safe. They can be used to access your account
                if you lose access to your authenticator app.
              </Text>
              <Group gap="0.5rem" align="center" justify="center">
                {recoveryCodes.map((code, index) => (
                  <Code key={index}>{code}</Code>
                ))}
              </Group>
              <CopyButton value={recoveryCodes.join("\n")}>
                {({ copied, copy }) => (
                  <Button
                    size="compact-sm"
                    color={copied ? "teal" : "blue"}
                    onClick={() => {
                      copy();
                      notifications.show({
                        color: "teal",
                        message: "Recovery codes copied to clipboard",
                      });
                    }}
                  >
                    Copy
                  </Button>
                )}
              </CopyButton>
            </Stack>
          )}
          <Stack gap="0.5rem">
            <Button
              variant="filled"
              onClick={() =>
                doSetTwoFactorAuth.mutate({
                  enable: false,
                  resetSharedKey: true,
                  resetRecoveryCodes: true,
                  forgetMachine: true,
                } as TwoFactorAuthRequest)
              }
            >
              Disable
            </Button>
            <Button
              variant="default"
              onClick={() =>
                doSetTwoFactorAuth.mutate({
                  resetSharedKey: false,
                  resetRecoveryCodes: true,
                  forgetMachine: false,
                } as TwoFactorAuthRequest)
              }
            >
              Reset Recovery Codes
            </Button>
          </Stack>
        </Stack>
      );
    }

    if (showAuthenticatorSetup) {
      return (
        <Stack>
          <Stack gap="0.5rem" align="center">
            <Text size="sm">
              Scan this code with your authenticator app or enter the manual
              code below.
            </Text>
            <QRCodeSVG
              value={buildAuthenticatorUrl(
                twoFactorAuthQuery.data?.sharedKey ?? ""
              )}
              bgColor="var(--mantine-color-default)"
              fgColor="var(--mantine-color-text)"
            />
            <Group>
              <Code>{formatKey(twoFactorAuthQuery.data?.sharedKey ?? "")}</Code>
              <CopyButton
                value={formatKey(twoFactorAuthQuery.data?.sharedKey ?? "")}
              >
                {({ copied, copy }) => (
                  <Button
                    size="compact-sm"
                    color={copied ? "teal" : "blue"}
                    onClick={() => {
                      copy();
                      notifications.show({
                        color: "teal",
                        message: "Code copied to clipboard",
                      });
                    }}
                  >
                    Copy
                  </Button>
                )}
              </CopyButton>
            </Group>
          </Stack>
          <Stack justify="center" align="center" gap="0.5rem">
            <Text size="sm">
              Then, enter the 6-digit code from your authenticator app.
            </Text>
            <PinInput
              length={6}
              type="number"
              autoFocus
              value={validationCodeField.getValue()}
              onChange={(value) => validationCodeField.setValue(value)}
            />
          </Stack>
          <Button
            onClick={() =>
              doSetTwoFactorAuth.mutate({
                enable: true,
                twoFactorCode: validationCodeField.getValue(),
                resetSharedKey: false,
                resetRecoveryCodes: true,
                forgetMachine: true,
              } as TwoFactorAuthRequest)
            }
          >
            Enable 2FA
          </Button>
        </Stack>
      );
    }

    return <Button onClick={toggle}>Setup 2FA</Button>;
  };

  if (twoFactorAuthQuery.isPending) {
    return <Skeleton height={300} radius="md" className={classes.skeleton} />;
  }

  return (
    <Card className={classes.card} withBorder radius="md" shadow="sm">
      <LoadingOverlay visible={doSetTwoFactorAuth.isPending} />
      <CardSection className={classes.cardSection}>
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
        {getAuthenticatorCardContent()}
      </CardSection>
    </Card>
  );
};

export default TwoFactorAuth;
