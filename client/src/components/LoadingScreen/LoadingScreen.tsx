import React from "react";

const LoadingScreen = (): React.ReactNode => {
  return (
    <>
      <style>{`
        .loading-container {
          display: flex;
          justify-content: center;
          align-items: center;
          height: 100vh;
          width: 100vw;
          background-color: #FFFFFE;
        }
        .loading-spinner {
          width: 100px;
          height: 100px;
          border: 15px solid #e0e0e0;
          border-top: 15px solid #5c7cfa;
          border-radius: 50%;
          animation: spin 1s linear infinite;
        }
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
        @media (prefers-color-scheme: dark) {
          .loading-container {
            background-color: #121212;
          }
          .loading-spinner {
            border-color: #343434;
            border-top-color: #5c7cfa;
          }
        }
      `}</style>
      <div className="loading-container">
        <div className="loading-spinner" />
      </div>
    </>
  );
};

export default LoadingScreen;
