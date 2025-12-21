import React from "react";

const SuspenseLoadingScreen = (): React.ReactNode => {
  return (
    <>
      <style>{`
        .suspense-loading-container {
          display: flex;
          justify-content: center;
          align-items: center;
          height: 100vh;
          width: 100vw;
          background-color: #FFFFFE;
        }
        .suspense-loading-spinner {
          width: 60px;
          height: 60px;
          border: 6px solid #e0e0e0;
          border-top: 6px solid #5c7cfa;
          border-radius: 50%;
          animation: spin 1s linear infinite;
        }
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
        @media (prefers-color-scheme: dark) {
          .suspense-loading-container {
            background-color: #121212;
          }
          .suspense-loading-spinner {
            border-color: #343434;
            border-top-color: #5c7cfa;
          }
        }
      `}</style>
      <div className="suspense-loading-container">
        <div className="suspense-loading-spinner" />
      </div>
    </>
  );
};

export default SuspenseLoadingScreen;
