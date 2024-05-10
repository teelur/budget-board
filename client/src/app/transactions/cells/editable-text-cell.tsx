import { Input } from '@/components/ui/input';
import { type Transaction } from '@/types/transaction';
import React from 'react';

interface EditableTextCellProps {
  merchant: string;
  isSelected: boolean;
  isError: boolean;
  editCell: ((newTransaction: Transaction) => void) | undefined;
  rowTransaction: Transaction;
}

const EditableMerchantCell = (props: EditableTextCellProps): JSX.Element => {
  const [merchantValue, setMerchantValue] = React.useState(props.merchant);

  React.useEffect(() => {
    if (props.isError) {
      setMerchantValue(props.merchant);
    }
  }, [props.isError]);

  const onTextChange = (): void => {
    const newTransaction: Transaction = {
      ...props.rowTransaction,
      merchantName: merchantValue,
    };
    if (props.editCell != null) {
      props.editCell(newTransaction);
    }
  };

  if (props.isSelected) {
    return (
      <Input
        value={merchantValue}
        onChange={(e) => {
          setMerchantValue(e.target.value);
        }}
        onBlur={onTextChange}
        onClick={(e) => {
          e.stopPropagation();
        }}
        type="text"
      />
    );
  } else {
    return <span>{merchantValue}</span>;
  }
};

export default EditableMerchantCell;
