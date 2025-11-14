import React from "react";
import BudgetBoardLogo from "~/assets/budget-board-logo";
import {
  Container,
  Text,
  Stack,
  Group,
  Anchor,
  useComputedColorScheme,
  Card,
  Flex,
} from "@mantine/core";
import Register from "./Register";
import Login from "./Login";
import ResetPassword from "./ResetPassword";
import LoginWith2fa from "./LoginWith2fa";
import LoginWithRecovery from "./LoginWithRecovery";
import { getProjectEnvVariables } from "~/shared/projectEnvVariables";

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
        return <>There was an error.</>;
    }
  };

  return (
    <Flex bg="var(--mantine-color-content-background)" w="100%" h="100vh">
      <Container size="lg" w="500px" maw="100%">
        <Stack align="center" gap={5}>
          <Text size="lg" fw={600}>
            Welcome to
          </Text>
          <BudgetBoardLogo
            width={340}
            darkMode={computedColorScheme === "dark"}
          />
          <Text c="dimmed" size="md" fw={600}>
            A simple app for managing monthly budgets.
          </Text>
        </Stack>
        <Card shadow="sm" withBorder mt="2em" w="100%" maw="500px" p={20}>
          {getCardState()}
        </Card>
        {loginCardState !== LoginCardState.Register &&
          envVariables.VITE_DISABLE_NEW_USERS?.toLowerCase() !== "true" &&
          envVariables.VITE_DISABLE_LOCAL_AUTH?.toLowerCase() !== "true" && (
            <Group mt="xl" justify="center">
              <Text size="sm" fw={600}>
                Don't have an account?
              </Text>
              <Anchor
                size="sm"
                fw={600}
                onClick={() => setLoginCardState(LoginCardState.Register)}
              >
                Register here
              </Anchor>
            </Group>
          )}
      </Container>
    </Flex>
  );
};

export default Welcome;
