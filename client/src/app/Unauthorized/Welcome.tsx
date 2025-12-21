import React from "react";
import BudgetBoardLogo from "~/assets/budget-board-logo";
import { Stack, Group, Anchor, useComputedColorScheme } from "@mantine/core";
import Register from "./Register/Register";
import Login from "./Login/Login";
import ResetPassword from "./ResetPassword/ResetPassword";
import LoginWith2fa from "./LoginWith2fa/LoginWith2fa";
import LoginWithRecovery from "./LoginWithRecovery/LoginWithRecovery";
import { getProjectEnvVariables } from "~/shared/projectEnvVariables";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Card from "~/components/core/Card/Card";
import { useTranslation } from "react-i18next";

export enum LoginCardState {
  Login,
  LoginWith2fa,
  LoginWithRecovery,
  ResetPassword,
  Register,
}

const Welcome = (): React.ReactNode => {
  const [loginCardState, setLoginCardState] = React.useState<LoginCardState>(
    LoginCardState.Login
  );
  const [userEmail, setUserEmail] = React.useState<string>("");
  const [userPassword, setUserPassword] = React.useState<string>("");

  const { t } = useTranslation();

  const computedColorScheme = useComputedColorScheme();

  const { envVariables } = getProjectEnvVariables();

  const getCardState = (): React.ReactNode => {
    switch (loginCardState) {
      case LoginCardState.Login:
        return (
          <Login
            setLoginCardState={setLoginCardState}
            setUserEmail={setUserEmail}
            setUserPassword={setUserPassword}
          />
        );
      case LoginCardState.LoginWith2fa:
        return (
          <LoginWith2fa
            setLoginCardState={setLoginCardState}
            userEmail={userEmail}
            userPassword={userPassword}
          />
        );
      case LoginCardState.LoginWithRecovery:
        return (
          <LoginWithRecovery
            setLoginCardState={setLoginCardState}
            userEmail={userEmail}
            userPassword={userPassword}
          />
        );
      case LoginCardState.ResetPassword:
        return (
          <ResetPassword
            setLoginCardState={setLoginCardState}
            email={userEmail}
          />
        );
      case LoginCardState.Register:
        return <Register setLoginCardState={setLoginCardState} />;
      default:
        return <>{t("error_occurred_while_loading_message")}</>;
    }
  };

  return (
    <Group
      bg="var(--background-color-base)"
      w="100%"
      h="100vh"
      justify="center"
    >
      <Stack w="500px" maw="100%" align="center">
        <Stack align="center" gap="0.25rem">
          <PrimaryText size="lg">{t("welcome_to")}</PrimaryText>
          <BudgetBoardLogo
            width={340}
            darkMode={computedColorScheme === "dark"}
          />
          <DimmedText size="md">
            {t("a_simple_app_for_managing_monthly_budgets")}
          </DimmedText>
        </Stack>
        <Card
          p="1rem"
          w="100%"
          maw={{ base: "95%", sm: "500px" }}
          elevation={1}
        >
          {getCardState()}
        </Card>
        {loginCardState !== LoginCardState.Register &&
          envVariables.VITE_DISABLE_NEW_USERS?.toLowerCase() !== "true" &&
          envVariables.VITE_DISABLE_LOCAL_AUTH?.toLowerCase() !== "true" && (
            <Group mt="xl" justify="center">
              <DimmedText size="sm">{t("dont_have_an_account")}</DimmedText>
              <Anchor
                size="sm"
                fw={600}
                onClick={() => setLoginCardState(LoginCardState.Register)}
              >
                {t("register_here")}
              </Anchor>
            </Group>
          )}
      </Stack>
    </Group>
  );
};

export default Welcome;
