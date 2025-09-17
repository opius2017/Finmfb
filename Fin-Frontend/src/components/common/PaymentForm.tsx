import React, { useState, useEffect } from 'react';
import { 
  useInitiatePaymentMutation, 
  useLazyVerifyPaymentQuery,
  loadPaymentSdk 
} from '../../services/paymentApi';
import { Button, Card, Form, Input, Select, Spin, Alert, Modal } from 'antd';
import toast from 'react-hot-toast';

const PaymentForm = ({ 
  amount, 
  customerEmail, 
  customerId, 
  description, 
  onSuccess, 
  onCancel,
  metadata = {},
  disableEmailEdit = false
}) => {
  const [form] = Form.useForm();
  const [paymentMethod, setPaymentMethod] = useState('card');
  const [paymentProvider, setPaymentProvider] = useState('paystack');
  const [sdkLoaded, setSdkLoaded] = useState(false);
  const [paymentProcessing, setPaymentProcessing] = useState(false);
  const [showVerification, setShowVerification] = useState(false);
  const [reference, setReference] = useState('');
  
  const [initiatePayment, { isLoading: isInitiating }] = useInitiatePaymentMutation();
  const [verifyPayment, { isLoading: isVerifying }] = useLazyVerifyPaymentQuery();
  
  useEffect(() => {
    // Pre-fill form values
    form.setFieldsValue({
      amount,
      email: customerEmail,
      paymentMethod,
      paymentProvider
    });
    
    // Load the selected payment SDK
    loadSelectedSdk();
  }, []);
  
  const loadSelectedSdk = async () => {
    try {
      await loadPaymentSdk(paymentProvider);
      setSdkLoaded(true);
    } catch (error) {
      toast.error(`Failed to load payment provider: ${error.message}`);
    }
  };
  
  const handleProviderChange = async (value) => {
    setPaymentProvider(value);
    setSdkLoaded(false);
    
    try {
      await loadPaymentSdk(value);
      setSdkLoaded(true);
    } catch (error) {
      toast.error(`Failed to load payment provider: ${error.message}`);
    }
  };
  
  const handleSubmit = async (values) => {
    try {
      setPaymentProcessing(true);
      
      // Generate channels array based on payment method
      const channels = [];
      if (values.paymentMethod === 'card') {
        channels.push('card');
      } else if (values.paymentMethod === 'bank') {
        channels.push('bank');
      } else if (values.paymentMethod === 'ussd') {
        channels.push('ussd');
      } else if (values.paymentMethod === 'mobile_money') {
        channels.push('mobile_money');
      }
      
      // Create payment request
      const paymentRequest = {
        amount: parseFloat(values.amount),
        currency: 'NGN',
        customerEmail: values.email,
        reference: `TX-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
        callbackUrl: window.location.origin + '/payment/callback',
        metadata: {
          ...metadata,
          customerId,
          description
        },
        paymentChannels: channels
      };
      
      // Store reference for verification
      setReference(paymentRequest.reference);
      
      // Initiate payment on backend
      const response = await initiatePayment(paymentRequest).unwrap();
      
      if (response.status && response.data) {
        // Redirect to payment page
        window.location.href = response.data.authorizationUrl;
      } else {
        toast.error(response.message || 'Failed to initiate payment');
        setPaymentProcessing(false);
      }
    } catch (error) {
      toast.error(`Payment initiation failed: ${error.message}`);
      setPaymentProcessing(false);
    }
  };
  
  const checkPaymentStatus = async () => {
    if (!reference) {
      toast.error('No payment reference found');
      return;
    }
    
    try {
      const result = await verifyPayment(reference).unwrap();
      
      if (result.status && result.data) {
        if (result.data.status === 'success') {
          toast.success('Payment successful!');
          setShowVerification(false);
          
          // Call the success callback with payment details
          if (onSuccess) {
            onSuccess({
              reference: result.data.reference,
              amount: result.data.amount / 100,
              transactionId: result.data.id,
              paymentChannel: result.data.paymentChannel,
              customerEmail: result.data.customer?.email,
              paidAt: result.data.paidAt
            });
          }
        } else if (result.data.status === 'pending') {
          toast('Payment is still processing. Please wait a moment and try again.');
        } else {
          toast.error(`Payment failed with status: ${result.data.status}`);
          if (onCancel) {
            onCancel();
          }
        }
      } else {
        toast.error(result.message || 'Failed to verify payment');
      }
    } catch (error) {
      toast.error(`Payment verification failed: ${error.message}`);
    }
  };
  
  return (
    <Card title="Payment Details" loading={!sdkLoaded}>
      {paymentProcessing ? (
        <div style={{ textAlign: 'center', padding: '2rem' }}>
          <Spin size="large" />
          <p style={{ marginTop: '1rem' }}>Processing your payment...</p>
          <Button 
            type="link" 
            onClick={() => setShowVerification(true)}
            style={{ marginTop: '1rem' }}
          >
            I've completed payment
          </Button>
        </div>
      ) : (
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            name="amount"
            label="Amount (₦)"
            rules={[{ required: true, message: 'Please enter amount' }]}
          >
            <Input 
              type="number" 
              min={100} 
              step={100} 
              disabled={amount > 0}
              prefix="₦" 
            />
          </Form.Item>
          
          <Form.Item
            name="email"
            label="Email"
            rules={[
              { required: true, message: 'Please enter email' },
              { type: 'email', message: 'Please enter a valid email' }
            ]}
          >
            <Input disabled={disableEmailEdit} />
          </Form.Item>
          
          <Form.Item
            name="paymentProvider"
            label="Payment Provider"
          >
            <Select onChange={handleProviderChange}>
              <Select.Option value="paystack">Paystack</Select.Option>
              <Select.Option value="flutterwave">Flutterwave</Select.Option>
            </Select>
          </Form.Item>
          
          <Form.Item
            name="paymentMethod"
            label="Payment Method"
          >
            <Select onChange={(value) => setPaymentMethod(value)}>
              <Select.Option value="card">Card Payment</Select.Option>
              <Select.Option value="bank">Bank Transfer</Select.Option>
              <Select.Option value="ussd">USSD</Select.Option>
              <Select.Option value="mobile_money">Mobile Money</Select.Option>
            </Select>
          </Form.Item>
          
          {description && (
            <Alert
              message="Payment Information"
              description={description}
              type="info"
              showIcon
              style={{ marginBottom: '1rem' }}
            />
          )}
          
          <Form.Item>
            <Button 
              type="primary" 
              htmlType="submit" 
              loading={isInitiating}
              block
            >
              Make Payment
            </Button>
          </Form.Item>
          
          {onCancel && (
            <Button 
              onClick={onCancel}
              block
              danger
            >
              Cancel
            </Button>
          )}
        </Form>
      )}
      
      <Modal
        title="Verify Payment"
        visible={showVerification}
        onCancel={() => setShowVerification(false)}
        footer={[
          <Button key="cancel" onClick={() => setShowVerification(false)}>
            Close
          </Button>,
          <Button 
            key="check" 
            type="primary" 
            loading={isVerifying}
            onClick={checkPaymentStatus}
          >
            Check Payment Status
          </Button>
        ]}
      >
        <p>Click the button below to verify your payment status.</p>
        <p>Reference: {reference}</p>
      </Modal>
    </Card>
  );
};

export default PaymentForm;