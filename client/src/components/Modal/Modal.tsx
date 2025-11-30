import baseClasses from "~/styles/Base.module.css";

import React from "react";
import {
  Modal as MantineModal,
  ModalProps as MantineModalProps,
} from "@mantine/core";

interface ModalProps extends MantineModalProps {
  children?: React.ReactNode;
}

const Modal = ({ children, ...props }: ModalProps): React.ReactNode => {
  return (
    <MantineModal
      classNames={{
        content: `${baseClasses.modalRoot} ${baseClasses.modal}`,
        header: baseClasses.modal,
      }}
      styles={{
        inner: {
          left: "0",
          right: "0",
          padding: "0 !important",
        },
      }}
      {...props}
    >
      {children}
    </MantineModal>
  );
};

export default Modal;
