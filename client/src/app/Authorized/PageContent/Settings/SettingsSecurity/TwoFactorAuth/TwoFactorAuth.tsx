import { Group, Stack, CopyButton } from "@mantine/core";
import {
  Badge,
  BodyText,
  Button,
  Card,
  Code,
  HeadingText,
  LoadingOverlay,
  PinInput,
  Skeleton,
} from "@teelur/budget-board-ui";
import React from "react";
import { useTwoFactorAuthenticationQuery } from "~/hooks/queries/useTwoFactorAuthenticationQuery";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { useField } from "@mantine/form";
import { QRCodeSVG } from "qrcode.react";
import { useDisclosure } from "@mantine/hooks";
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
              <HeadingText tone="primary" level={6}>
                {t("recovery_codes")}
              </HeadingText>
              <BodyText tone="secondary" size="sm">
                {t("two_factor_auth_recovery_codes_info")}
              </BodyText>
              <Group gap="0.5rem" align="center" justify="center">
                {recoveryCodes.map((code, index) => (
                  <Code key={index}>{code}</Code>
                ))}
              </Group>
              <CopyButton value={recoveryCodes.join("\n")}>
                {({ copy }) => (
                  <Button
                    variant="filled"
                    color="primary"
                    size="xs"
                    onClick={() => {
                      copy();
                      showNotification({
                        type: NotificationType.Information,
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
          <Group gap="0.5rem">
            <Button
              variant="filled"
              color="error"
              size="xs"
              flex="1 1 0"
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
              variant="filled"
              color="primary"
              size="xs"
              flex="1 1 0"
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
          </Group>
        </Stack>
      );
    }

    if (showAuthenticatorSetup) {
      return (
        <Stack>
          <Stack gap="0.5rem" align="center">
            <BodyText tone="secondary" size="sm">
              {t("scan_the_qr_code_with_your_authenticator_app")}
            </BodyText>
            <QRCodeSVG
              value={buildAuthenticatorUrl(
                twoFactorAuthQuery.data?.sharedKey ?? "",
              )}
              bgColor="var(--bbui-color-surface)"
              fgColor="var(--bbui-color-text-heading)"
            />
            <Group>
              <Code>{formatKey(twoFactorAuthQuery.data?.sharedKey ?? "")}</Code>
              <CopyButton
                value={formatKey(twoFactorAuthQuery.data?.sharedKey ?? "")}
              >
                {({ copy }) => (
                  <Button
                    variant="filled"
                    color="primary"
                    size="xs"
                    onClick={() => {
                      copy();
                      showNotification({
                        type: NotificationType.Information,
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
            <BodyText tone="primary" size="sm" fw={500}>
              {t("enter_the_verification_code_from_your_authenticator_app")}
            </BodyText>
            <PinInput
              length={6}
              type="number"
              inputMode="numeric"
              oneTimeCode
              ariaLabel={t(
                "enter_the_verification_code_from_your_authenticator_app",
              )}
              getInputProps={(index) =>
                index === 0 ? { name: "two-factor-code" } : {}
              }
              autoFocus
              value={validationCodeField.getValue()}
              onChange={(value) => validationCodeField.setValue(value)}
            />
          </Stack>
          <Button
            variant="filled"
            color="primary"
            size="xs"
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

    return (
      <Button variant="filled" color="primary" size="xs" onClick={toggle}>
        {t("setup_2fa")}
      </Button>
    );
  };

  if (twoFactorAuthQuery.isPending) {
    return <Skeleton height={300} />;
  }

  return (
    <Card>
      <LoadingOverlay visible={setTwoFactorAuthenticationMutation.isPending} />
      <Stack gap="1rem">
        <Group gap="1rem">
          <HeadingText tone="heading" level={4}>
            {t("two_factor_authentication")}
          </HeadingText>
          {twoFactorAuthQuery.data?.isTwoFactorEnabled ? (
            <Badge variant="light" color="success" size="xs">
              {t("enabled")}
            </Badge>
          ) : (
            <Badge variant="light" color="error" size="xs">
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
