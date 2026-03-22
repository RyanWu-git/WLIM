-- Function to find a common private session between two users
CREATE OR REPLACE FUNCTION find_common_session(user1_id UUID, user2_id UUID)
RETURNS TABLE (session_id UUID) AS $$
BEGIN
  RETURN QUERY
  SELECT p1.session_id
  FROM session_participants p1
  JOIN session_participants p2 ON p1.session_id = p2.session_id
  JOIN chat_sessions s ON p1.session_id = s.id
  WHERE p1.user_id = user1_id
    AND p2.user_id = user2_id
    AND s.type = 'private';
END;
$$ LANGUAGE plpgsql;

-- Trigger to update chat_sessions.last_message_at when a new message is inserted
CREATE OR REPLACE FUNCTION update_last_message_at()
RETURNS TRIGGER AS $$
BEGIN
  UPDATE chat_sessions
  SET last_message_at = NEW.created_at
  WHERE id = NEW.session_id;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_last_message_at
AFTER INSERT ON messages
FOR EACH ROW
EXECUTE FUNCTION update_last_message_at();
