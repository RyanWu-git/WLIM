import React, { useEffect, useState } from 'react';
import { supabase } from '@/lib/supabase';
import { useAuthStore } from '@/hooks/useAuth';

interface Friend {
  id: string;
  username: string;
  avatar_url: string | null;
  email: string;
}

interface FriendListProps {
  onSelectSession: (id: string) => void;
}

const FriendList: React.FC<FriendListProps> = ({ onSelectSession }) => {
  const [friends, setFriends] = useState<Friend[]>([]);
  const [loading, setLoading] = useState(true);
  const user = useAuthStore((state) => state.user);

  useEffect(() => {
    if (!user) return;

    const fetchFriends = async () => {
      setLoading(true);
      try {
        // Fetch all users except current user for now as "friends"
        const { data, error } = await supabase
          .from('users')
          .select('*')
          .neq('id', user.id);

        if (error) throw error;
        setFriends(data || []);
      } catch (err) {
        console.error('Error fetching friends:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchFriends();
  }, [user]);

  const handleStartChat = async (friendId: string) => {
    if (!user) return;

    try {
      // Find or create private chat session
      const { data: existingSessions, error: sessionError } = await supabase
        .from('session_participants')
        .select('session_id')
        .eq('user_id', user.id);

      if (sessionError) throw sessionError;

      // For simplicity, let's create a new session or check existing ones
      // This is a bit complex for a basic implementation, so let's keep it simple
      // and just create a new one for now if it doesn't exist.
      
      // Better way: query sessions that have both participants
      const { data: commonSessions, error: commonError } = await supabase.rpc('find_common_session', {
        user1_id: user.id,
        user2_id: friendId
      });

      if (commonError) {
        // Fallback: search manually if rpc not defined
        console.warn('RPC find_common_session failed, creating new session');
      }

      if (commonSessions && commonSessions.length > 0) {
        onSelectSession(commonSessions[0].session_id);
      } else {
        // Create new session
        const { data: newSession, error: createError } = await supabase
          .from('chat_sessions')
          .insert({ type: 'private', name: 'Private Chat' })
          .select()
          .single();

        if (createError) throw createError;

        await supabase.from('session_participants').insert([
          { session_id: newSession.id, user_id: user.id },
          { session_id: newSession.id, user_id: friendId },
        ]);

        onSelectSession(newSession.id);
      }
    } catch (err) {
      console.error('Error starting chat:', err);
    }
  };

  if (loading) {
    return <div className="p-4 text-center text-gray-400">正在加载好友...</div>;
  }

  if (friends.length === 0) {
    return <div className="p-4 text-center text-gray-400">暂无好友</div>;
  }

  return (
    <div className="flex flex-col">
      {friends.map((friend) => (
        <button
          key={friend.id}
          onClick={() => handleStartChat(friend.id)}
          className="flex items-center p-4 hover:bg-gray-700 transition-colors"
        >
          <div className="w-12 h-12 rounded-full bg-gray-600 flex items-center justify-center mr-3 font-bold">
            {friend.username?.[0]?.toUpperCase() || friend.email?.[0].toUpperCase() || 'F'}
          </div>
          <div className="flex-1 text-left">
            <h3 className="text-sm font-semibold truncate text-white">
              {friend.username || friend.email}
            </h3>
            <p className="text-xs text-gray-400 truncate">
              {friend.email}
            </p>
          </div>
        </button>
      ))}
    </div>
  );
};

export default FriendList;
