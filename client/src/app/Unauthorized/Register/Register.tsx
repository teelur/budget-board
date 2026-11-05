import { Stack, Group } from "@mantine/core";
import {
  Button,
  Card,
  PasswordInput,
  TextInput,
} from "@teelur/budget-board-ui";
import { hasLength, isEmail, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import { useTranslation } from "react-i18next";
import { useRegisterMutation } from "~/hooks/mutations/auth/useRegisterMutation";

interface RegisterProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
}

const Register = (props: RegisterProps): React.ReactNode => {
  const { t } = useTranslation();
  const registerMutation = useRegisterMutation();

  const usernameField = useField<string>({
    initialValue: "",
    validate: hasLength({ min: 1 }, t("username_required_message")),
  });
  const emailField = useField<string>({
    initialValue: "",
    validate: (value) =>
      value.trim() === "" ? null : isEmail(t("invalid_email_message"))(value),
  });

  const passwordMinLength = 3;
  const passwordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: passwordMinLength },
      t("password_min_length_message", {
        minLength: passwordMinLength,
      }),
    ),
  });
  const confirmPasswordField = useField<string>({
    initialValue: "",
    validate: (value) =>
      value !== passwordField.getValue() ? t("passwords_do_not_match") : null,
  });

  return (
    <Card.Section>
      <Stack align="center" gap="1rem" w="100%">
        <Stack gap="0.5rem" w="100%">
          <TextInput
            label={t("username")}
            w="100%"
            {...usernameField.getInputProps()}
          />
          <TextInput
            label={t("email_address_optional")}
            w="100%"
            {...emailField.getInputProps()}
          />
          <PasswordInput
            label={t("password")}
            w="100%"
            {...passwordField.getInputProps()}
          />
          <PasswordInput
            label={t("confirm_password")}
            w="100%"
            {...confirmPasswordField.getInputProps()}
          />
        </Stack>
        <Group gap="0.5rem" w="100%">
          <Button
            variant="filled"
            color="neutral"
            size="compact-sm"
            flex="1 1 0"
            onClick={() => props.setLoginCardState(LoginCardState.Login)}
          >
            {t("back_to_login")}
          </Button>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            flex="1 1 0"
            loading={registerMutation.isPending}
            onClick={() => {
              usernameField.validate();
              emailField.validate();
              passwordField.validate();
              confirmPasswordField.validate();

              if (
                usernameField.error ||
                emailField.error ||
                passwordField.error ||
                confirmPasswordField.error
              ) {
                return;
              }

              registerMutation.mutate(
                {
                  username: usernameField.getValue(),
                  email: emailField.getValue() || undefined,
                  password: passwordField.getValue(),
                },
                {
                  onSuccess: () => {
                    props.setLoginCardState(LoginCardState.Login);
                  },
                },
              );
            }}
          >
            {t("register")}
          </Button>
        </Group>
      </Stack>
    </Card.Section>
  );
};

export default Register;
