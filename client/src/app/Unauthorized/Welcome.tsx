import React from "react";
import BudgetBoardLogo from "~/assets/budget-board-logo";
import { Stack, Group, Anchor, useComputedColorScheme } from "@mantine/core";
import Register from "./Register";
import Login from "./Login";
import ResetPassword from "./ResetPassword";
import LoginWith2fa from "./LoginWith2fa";
import LoginWithRecovery from "./LoginWithRecovery";
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
        return <>{t("unauthorized.welcome.error_loading_page")}</>;
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
          <PrimaryText size="lg">
            {t("unauthorized.welcome.header")}
          </PrimaryText>
          <BudgetBoardLogo
            width={340}
            darkMode={computedColorScheme === "dark"}
          />
          <DimmedText size="md">
            {t("unauthorized.welcome.subtitle")}
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
              <DimmedText size="sm">
                {t("unauthorized.welcome.no_account")}
              </DimmedText>
              <Anchor
                size="sm"
                fw={600}
                onClick={() => setLoginCardState(LoginCardState.Register)}
              >
                {t("unauthorized.welcome.register_here")}
              </Anchor>
            </Group>
          )}
      </Stack>
    </Group>
  );
};

export default Welcome;
