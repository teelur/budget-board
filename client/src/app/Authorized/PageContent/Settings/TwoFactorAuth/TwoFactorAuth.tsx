import {
  Badge,
  Group,
  LoadingOverlay,
  Button,
  Stack,
  CopyButton,
  Skeleton,
} from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import { AxiosError, AxiosResponse } from "axios";
import { useField } from "@mantine/form";
import { QRCodeSVG } from "qrcode.react";
import { useDisclosure } from "@mantine/hooks";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PinInput from "~/components/core/Input/PinInput/PinInput";
import Code from "~/components/core/Code/Code";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

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

  const { request } = useAuth();

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
              <PrimaryText size="md">Recovery Codes</PrimaryText>
              <DimmedText size="sm">
                Keep these codes safe. They can be used to access your account
                if you lose access to your authenticator app.
              </DimmedText>
              <Group gap="0.5rem" align="center" justify="center">
                {recoveryCodes.map((code, index) => (
                  <Code key={index} elevation={1}>
                    {code}
                  </Code>
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
              bg="var(--button-color-destructive)"
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
              variant="outline"
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
            <PrimaryText size="sm">
              Scan this code with your authenticator app or enter the manual
              code below.
            </PrimaryText>
            <QRCodeSVG
              value={buildAuthenticatorUrl(
                twoFactorAuthQuery.data?.sharedKey ?? ""
              )}
              bgColor="var(--background-color-surface)"
              fgColor="var(--base-color-text-primary)"
            />
            <Group>
              <Code elevation={1}>
                {formatKey(twoFactorAuthQuery.data?.sharedKey ?? "")}
              </Code>
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
            <PrimaryText size="sm">
              Then, enter the 6-digit code from your authenticator app.
            </PrimaryText>
            <PinInput
              length={6}
              type="number"
              autoFocus
              value={validationCodeField.getValue()}
              onChange={(value) => validationCodeField.setValue(value)}
              elevation={1}
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
    return <Skeleton height={300} radius="md" />;
  }

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doSetTwoFactorAuth.isPending} />
      <Stack gap="1rem">
        <Group gap="1rem">
          <PrimaryText size="lg">Two Factor Authentication</PrimaryText>
          {twoFactorAuthQuery.data?.isTwoFactorEnabled ? (
            <Badge color="var(--button-color-confirm)" maw={80}>
              Enabled
            </Badge>
          ) : (
            <Badge color="var(--button-color-destructive)" maw={85}>
              Disabled
            </Badge>
          )}
        </Group>
        {getAuthenticatorCardContent()}
      </Stack>
    </Card>
  );
};

export default TwoFactorAuth;
