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
import { useTwoFactorAuthenticationQuery } from "~/hooks/queries/useTwoFactorAuthenticationQuery";
import { notifications } from "@mantine/notifications";
import { useField } from "@mantine/form";
import { QRCodeSVG } from "qrcode.react";
import { useDisclosure } from "@mantine/hooks";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PinInput from "~/components/core/Input/PinInput/PinInput";
import Code from "~/components/core/Code/Code";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useSetTwoFactorAuthenticationMutation } from "~/hooks/mutations/auth/useSetTwoFactorAuthenticationMutation";
import { TwoFactorAuthRequest } from "~/models/twoFactorAuth";

const TwoFactorAuth = (): React.ReactNode => {
  const [recoveryCodes, setRecoveryCodes] = React.useState<string[]>([]);
  const [showAuthenticatorSetup, { toggle }] = useDisclosure();

  const { t } = useTranslation();
  const twoFactorAuthQuery = useTwoFactorAuthenticationQuery();
  const setTwoFactorAuthenticationMutation =
    useSetTwoFactorAuthenticationMutation();

  const validationCodeField = useField<string>({
    initialValue: "",
    validate: (value) => {
      if (!value) {
        return t("validation_code_is_required");
      }
      return null;
    },
  });

  const setTwoFactorAuth = (twoFactorAuthData: TwoFactorAuthRequest): void => {
    setTwoFactorAuthenticationMutation.mutate({
      twoFactorAuthData,
      setRecoveryCodes,
    });
  };

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
              <PrimaryText size="md">{t("recovery_codes")}</PrimaryText>
              <DimmedText size="sm">
                {t("two_factor_auth_recovery_codes_info")}
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
                        message: t("recovery_codes_copied_to_clipboard"),
                      });
                    }}
                  >
                    {t("copy_recovery_codes")}
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
                setTwoFactorAuth({
                  enable: false,
                  resetSharedKey: true,
                  resetRecoveryCodes: true,
                  forgetMachine: true,
                })
              }
            >
              {t("disable")}
            </Button>
            <Button
              variant="outline"
              onClick={() =>
                setTwoFactorAuth({
                  resetSharedKey: false,
                  resetRecoveryCodes: true,
                  forgetMachine: false,
                })
              }
            >
              {t("generate_new_recovery_codes")}
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
              {t("scan_the_qr_code_with_your_authenticator_app")}
            </PrimaryText>
            <QRCodeSVG
              value={buildAuthenticatorUrl(
                twoFactorAuthQuery.data?.sharedKey ?? "",
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
                        message: t("code_copied_to_clipboard"),
                      });
                    }}
                  >
                    {t("copy_key")}
                  </Button>
                )}
              </CopyButton>
            </Group>
          </Stack>
          <Stack justify="center" align="center" gap="0.5rem">
            <PrimaryText size="sm">
              {t("enter_the_verification_code_from_your_authenticator_app")}
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
              setTwoFactorAuth({
                enable: true,
                twoFactorCode: validationCodeField.getValue(),
                resetSharedKey: false,
                resetRecoveryCodes: true,
                forgetMachine: true,
              })
            }
          >
            {t("enable")}
          </Button>
        </Stack>
      );
    }

    return <Button onClick={toggle}>{t("setup_2fa")}</Button>;
  };

  if (twoFactorAuthQuery.isPending) {
    return <Skeleton height={300} radius="md" />;
  }

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={setTwoFactorAuthenticationMutation.isPending} />
      <Stack gap="1rem">
        <Group gap="1rem">
          <PrimaryText size="lg">{t("two_factor_authentication")}</PrimaryText>
          {twoFactorAuthQuery.data?.isTwoFactorEnabled ? (
            <Badge color="var(--button-color-confirm)">{t("enabled")}</Badge>
          ) : (
            <Badge color="var(--button-color-destructive)">
              {t("disabled")}
            </Badge>
          )}
        </Group>
        {getAuthenticatorCardContent()}
      </Stack>
    </Card>
  );
};

export default TwoFactorAuth;
