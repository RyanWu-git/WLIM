import React, { useEffect, useState } from 'react';
import { supabase } from '@/lib/supabase';
import { useAuthStore } from '@/hooks/useAuth';

interface ChatSession {
  id: string;
  type: 'private' | 'group';
  name: string | null;
  last_message_at: string;
  participants: string[];
}

interface ChatListProps {
  activeSessionId: string | null;
  onSelectSession: (id: string) => void;
}

const ChatList: React.FC<ChatListProps> = ({ activeSessionId, onSelectSession }) => {
  const [sessions, setSessions] = useState<ChatSession[]>([]);
  const [loading, setLoading] = useState(true);
  const user = useAuthStore((state) => state.user);

  useEffect(() => {
    if (!user) return;

    const fetchSessions = async () => {
      setLoading(true);
      try {
        // Fetch sessions where user is a participant
        const { data: participantData, error: participantError } = await supabase
          .from('session_participants')
          .select('session_id')
          .eq('user_id', user.id);

        if (participantError) throw participantError;

        if (participantData && participantData.length > 0) {
          const sessionIds = participantData.map((p) => p.session_id);
          const { data, error } = await supabase
            .from('chat_sessions')
            .select('*')
            .in('id', sessionIds)
            .order('last_message_at', { ascending: false });

          if (error) throw error;
          setSessions(data || []);
        }
      } catch (err) {
        console.error('Error fetching chat sessions:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchSessions();

    // Listen for new sessions
    const subscription = supabase
      .channel('public:chat_sessions')
      .on('postgres_changes', 
        { event: '*', schema: 'public', table: 'chat_sessions' },
        () => {
          fetchSessions();
        }
      )
      .subscribe();

    return () => {
      subscription.unsubscribe();
    };
  }, [user]);

  if (loading) {
    return <div className="p-4 text-center text-gray-400">正在加载会话...</div>;
  }

  if (sessions.length === 0) {
    return <div className="p-4 text-center text-gray-400">暂无聊天</div>;
  }

  return (
    <div className="flex flex-col">
      {sessions.map((session) => (
        <button
          key={session.id}
          onClick={() => onSelectSession(session.id)}
          className={`flex items-center p-4 hover:bg-gray-700 transition-colors border-l-4 ${
            activeSessionId === session.id
              ? 'bg-gray-700 border-blue-500'
              : 'border-transparent'
          }`}
        >
          <div className="w-12 h-12 rounded-full bg-gray-600 flex items-center justify-center mr-3 font-bold">
            {session.name?.[0]?.toUpperCase() || 'G'}
          </div>
          <div className="flex-1 min-w-0 text-left">
            <div className="flex justify-between items-baseline mb-1">
              <h3 className="text-sm font-semibold truncate text-white">
                {session.name || '未命名会话'}
              </h3>
              <span className="text-[10px] text-gray-400 shrink-0">
                {new Date(session.last_message_at).toLocaleTimeString([], {
                  hour: '2-digit',
                  minute: '2-digit',
                })}
              </span>
            </div>
            <p className="text-xs text-gray-400 truncate">
              {session.type === 'private' ? '私聊' : '群聊'}
            </p>
          </div>
        </button>
      ))}
    </div>
  );
};

export default ChatList;
