
CREATE TABLE mt_inbox_states (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    message_id uuid NOT NULL,
    consumer_id uuid NOT NULL,
    lock_id uuid NOT NULL,
    received timestamp with time zone NOT NULL,
    receive_count integer NOT NULL,
    expiration_time timestamp with time zone,
    consumed timestamp with time zone,
    delivered timestamp with time zone,
    last_sequence_number bigint,
    CONSTRAINT pk_inbox_states PRIMARY KEY (id),
    CONSTRAINT ak_inbox_state_message_id_consumer_id UNIQUE (message_id, consumer_id)
);

CREATE TABLE mt_outbox_messages (
    sequence_number bigint GENERATED BY DEFAULT AS IDENTITY,
    enqueue_time timestamp with time zone,
    sent_time timestamp with time zone NOT NULL,
    headers text,
    properties text,
    inbox_message_id uuid,
    inbox_consumer_id uuid,
    outbox_id uuid,
    message_id uuid NOT NULL,
    content_type character varying(256) NOT NULL,
    message_type text NOT NULL,
    body text NOT NULL,
    conversation_id uuid,
    correlation_id uuid,
    initiator_id uuid,
    request_id uuid,
    source_address character varying(256),
    destination_address character varying(256),
    response_address character varying(256),
    fault_address character varying(256),
    expiration_time timestamp with time zone,
    CONSTRAINT pk_outbox_messages PRIMARY KEY (sequence_number)
);

CREATE TABLE mt_outbox_states (
    outbox_id uuid NOT NULL,
    lock_id uuid NOT NULL,
    created timestamp with time zone NOT NULL,
    delivered timestamp with time zone,
    last_sequence_number bigint,
    CONSTRAINT pk_outbox_states PRIMARY KEY (outbox_id)
);