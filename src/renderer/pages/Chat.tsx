import React, { useState } from 'react';
import Sidebar from '@/components/Sidebar';
import ChatWindow from '@/components/ChatWindow';

const Chat: React.FC = () => {
  const [activeSessionId, setActiveSessionId] = useState<string | null>(null);

  return (
    <div className="flex h-screen overflow-hidden bg-gray-100">
      {/* Left Sidebar */}
      <Sidebar
        activeSessionId={activeSessionId}
        onSelectSession={setActiveSessionId}
      />

      {/* Right Chat Area */}
      <div className="flex-1 flex flex-col">
        {activeSessionId ? (
          <ChatWindow sessionId={activeSessionId} />
        ) : (
          <div className="flex items-center justify-center flex-1 bg-gray-50 text-gray-400">
            请选择一个聊天会话
          </div>
        )}
      </div>
    </div>
  );
};

export default Chat;
