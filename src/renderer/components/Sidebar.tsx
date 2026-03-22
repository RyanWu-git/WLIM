import React, { useState } from 'react';
import { MessageSquare, Users, LogOut } from 'lucide-react';
import { supabase } from '@/lib/supabase';
import { useAuthStore } from '@/hooks/useAuth';
import ChatList from './ChatList';
import FriendList from './FriendList';

interface SidebarProps {
  activeSessionId: string | null;
  onSelectSession: (id: string) => void;
}

const Sidebar: React.FC<SidebarProps> = ({ activeSessionId, onSelectSession }) => {
  const [activeTab, setActiveTab] = useState<'chats' | 'friends'>('chats');
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  const handleLogout = async () => {
    await supabase.auth.signOut();
    logout();
  };

  return (
    <div className="w-80 h-screen flex flex-col bg-[#2B2B2B] text-white">
      {/* Sidebar Header: Tabs */}
      <div className="flex justify-between items-center p-4 border-b border-gray-700">
        <div className="flex space-x-4">
          <button
            onClick={() => setActiveTab('chats')}
            className={`p-2 rounded hover:bg-gray-700 transition-colors ${
              activeTab === 'chats' ? 'bg-gray-700 text-blue-400' : 'text-gray-400'
            }`}
            title="聊天"
          >
            <MessageSquare size={24} />
          </button>
          <button
            onClick={() => setActiveTab('friends')}
            className={`p-2 rounded hover:bg-gray-700 transition-colors ${
              activeTab === 'friends' ? 'bg-gray-700 text-blue-400' : 'text-gray-400'
            }`}
            title="好友"
          >
            <Users size={24} />
          </button>
        </div>
        <button
          onClick={handleLogout}
          className="p-2 rounded hover:bg-gray-700 text-gray-400 hover:text-red-400 transition-colors"
          title="退出登录"
        >
          <LogOut size={20} />
        </button>
      </div>

      {/* List Area */}
      <div className="flex-1 overflow-y-auto">
        {activeTab === 'chats' ? (
          <ChatList
            activeSessionId={activeSessionId}
            onSelectSession={onSelectSession}
          />
        ) : (
          <FriendList onSelectSession={onSelectSession} />
        )}
      </div>

      {/* Sidebar Footer: User Info */}
      <div className="p-4 border-t border-gray-700 flex items-center space-x-3">
        <div className="w-10 h-10 rounded-full bg-gray-600 flex items-center justify-center text-lg font-bold">
          {user?.email?.[0].toUpperCase() || 'U'}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium truncate">{user?.email}</p>
        </div>
      </div>
    </div>
  );
};

export default Sidebar;
