import { useMutation } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export type LoginData = {
  email: string;
  password: string;
  rememberMe: boolean;
  twoFactorCode?: string;
  recoveryCode?: string;
};

export const useLoginMutation = () => {
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (loginData: LoginData) =>
      await request({
        url: "/api/login",
        method: "POST",
        data: {
          email: loginData.email,
          password: loginData.password,
          twoFactorCode: loginData.twoFactorCode,
          recoveryCode: loginData.recoveryCode,
        },
        params: {
          rememberMe: loginData.rememberMe,
        },
      }),
  });
};
