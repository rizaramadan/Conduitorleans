--
-- PostgreSQL database dump
--

-- Dumped from database version 12.2
-- Dumped by pg_dump version 12.2

-- Started on 2020-12-26 15:45:27

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 225 (class 1255 OID 51249)
-- Name: delete_reminder_row(character varying, character varying, character varying, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.delete_reminder_row(serviceidarg character varying, grainidarg character varying, remindernamearg character varying, versionarg integer) RETURNS TABLE(row_count integer)
    LANGUAGE plpgsql
    AS $$
DECLARE
    RowCountVar int := 0;
BEGIN


    DELETE FROM OrleansRemindersTable
    WHERE
        ServiceId = ServiceIdArg AND ServiceIdArg IS NOT NULL
        AND GrainId = GrainIdArg AND GrainIdArg IS NOT NULL
        AND ReminderName = ReminderNameArg AND ReminderNameArg IS NOT NULL
        AND Version = VersionArg AND VersionArg IS NOT NULL;

    GET DIAGNOSTICS RowCountVar = ROW_COUNT;

    RETURN QUERY SELECT RowCountVar;

END
$$;


ALTER FUNCTION public.delete_reminder_row(serviceidarg character varying, grainidarg character varying, remindernamearg character varying, versionarg integer) OWNER TO postgres;

--
-- TOC entry 222 (class 1255 OID 51232)
-- Name: insert_membership(character varying, character varying, integer, integer, character varying, character varying, integer, integer, timestamp without time zone, timestamp without time zone, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_membership(deploymentidarg character varying, addressarg character varying, portarg integer, generationarg integer, silonamearg character varying, hostnamearg character varying, statusarg integer, proxyportarg integer, starttimearg timestamp without time zone, iamalivetimearg timestamp without time zone, versionarg integer) RETURNS TABLE(row_count integer)
    LANGUAGE plpgsql
    AS $$
DECLARE
    RowCountVar int := 0;
BEGIN

    BEGIN
        INSERT INTO OrleansMembershipTable
        (
            DeploymentId,
            Address,
            Port,
            Generation,
            SiloName,
            HostName,
            Status,
            ProxyPort,
            StartTime,
            IAmAliveTime
        )
        SELECT
            DeploymentIdArg,
            AddressArg,
            PortArg,
            GenerationArg,
            SiloNameArg,
            HostNameArg,
            StatusArg,
            ProxyPortArg,
            StartTimeArg,
            IAmAliveTimeArg
        ON CONFLICT (DeploymentId, Address, Port, Generation) DO
            NOTHING;


        GET DIAGNOSTICS RowCountVar = ROW_COUNT;

        UPDATE OrleansMembershipVersionTable
        SET
            Timestamp = (now() at time zone 'utc'),
            Version = Version + 1
        WHERE
            DeploymentId = DeploymentIdArg AND DeploymentIdArg IS NOT NULL
            AND Version = VersionArg AND VersionArg IS NOT NULL
            AND RowCountVar > 0;

        GET DIAGNOSTICS RowCountVar = ROW_COUNT;

        ASSERT RowCountVar <> 0, 'no rows affected, rollback';


        RETURN QUERY SELECT RowCountVar;
    EXCEPTION
    WHEN assert_failure THEN
        RETURN QUERY SELECT RowCountVar;
    END;

END
$$;


ALTER FUNCTION public.insert_membership(deploymentidarg character varying, addressarg character varying, portarg integer, generationarg integer, silonamearg character varying, hostnamearg character varying, statusarg integer, proxyportarg integer, starttimearg timestamp without time zone, iamalivetimearg timestamp without time zone, versionarg integer) OWNER TO postgres;

--
-- TOC entry 209 (class 1255 OID 51231)
-- Name: insert_membership_version(character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_membership_version(deploymentidarg character varying) RETURNS TABLE(row_count integer)
    LANGUAGE plpgsql
    AS $$
DECLARE
    RowCountVar int := 0;
BEGIN

    BEGIN

        INSERT INTO OrleansMembershipVersionTable
        (
            DeploymentId
        )
        SELECT DeploymentIdArg
        ON CONFLICT (DeploymentId) DO NOTHING;

        GET DIAGNOSTICS RowCountVar = ROW_COUNT;

        ASSERT RowCountVar <> 0, 'no rows affected, rollback';

        RETURN QUERY SELECT RowCountVar;
    EXCEPTION
    WHEN assert_failure THEN
        RETURN QUERY SELECT RowCountVar;
    END;

END
$$;


ALTER FUNCTION public.insert_membership_version(deploymentidarg character varying) OWNER TO postgres;

--
-- TOC entry 208 (class 1255 OID 51230)
-- Name: update_i_am_alive_time(character varying, character varying, integer, integer, timestamp without time zone); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_i_am_alive_time(deployment_id character varying, address_arg character varying, port_arg integer, generation_arg integer, i_am_alive_time timestamp without time zone) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    -- This is expected to never fail by Orleans, so return value
    -- is not needed nor is it checked.
    UPDATE OrleansMembershipTable as d
    SET
        IAmAliveTime = i_am_alive_time
    WHERE
        d.DeploymentId = deployment_id AND deployment_id IS NOT NULL
        AND d.Address = address_arg AND address_arg IS NOT NULL
        AND d.Port = port_arg AND port_arg IS NOT NULL
        AND d.Generation = generation_arg AND generation_arg IS NOT NULL;
END
$$;


ALTER FUNCTION public.update_i_am_alive_time(deployment_id character varying, address_arg character varying, port_arg integer, generation_arg integer, i_am_alive_time timestamp without time zone) OWNER TO postgres;

--
-- TOC entry 223 (class 1255 OID 51233)
-- Name: update_membership(character varying, character varying, integer, integer, integer, character varying, timestamp without time zone, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_membership(deploymentidarg character varying, addressarg character varying, portarg integer, generationarg integer, statusarg integer, suspecttimesarg character varying, iamalivetimearg timestamp without time zone, versionarg integer) RETURNS TABLE(row_count integer)
    LANGUAGE plpgsql
    AS $$
DECLARE
    RowCountVar int := 0;
BEGIN

    BEGIN

    UPDATE OrleansMembershipVersionTable
    SET
        Timestamp = (now() at time zone 'utc'),
        Version = Version + 1
    WHERE
        DeploymentId = DeploymentIdArg AND DeploymentIdArg IS NOT NULL
        AND Version = VersionArg AND VersionArg IS NOT NULL;


    GET DIAGNOSTICS RowCountVar = ROW_COUNT;

    UPDATE OrleansMembershipTable
    SET
        Status = StatusArg,
        SuspectTimes = SuspectTimesArg,
        IAmAliveTime = IAmAliveTimeArg
    WHERE
        DeploymentId = DeploymentIdArg AND DeploymentIdArg IS NOT NULL
        AND Address = AddressArg AND AddressArg IS NOT NULL
        AND Port = PortArg AND PortArg IS NOT NULL
        AND Generation = GenerationArg AND GenerationArg IS NOT NULL
        AND RowCountVar > 0;


        GET DIAGNOSTICS RowCountVar = ROW_COUNT;

        ASSERT RowCountVar <> 0, 'no rows affected, rollback';


        RETURN QUERY SELECT RowCountVar;
    EXCEPTION
    WHEN assert_failure THEN
        RETURN QUERY SELECT RowCountVar;
    END;

END
$$;


ALTER FUNCTION public.update_membership(deploymentidarg character varying, addressarg character varying, portarg integer, generationarg integer, statusarg integer, suspecttimesarg character varying, iamalivetimearg timestamp without time zone, versionarg integer) OWNER TO postgres;

--
-- TOC entry 224 (class 1255 OID 51248)
-- Name: upsert_reminder_row(character varying, character varying, character varying, timestamp without time zone, bigint, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.upsert_reminder_row(serviceidarg character varying, grainidarg character varying, remindernamearg character varying, starttimearg timestamp without time zone, periodarg bigint, grainhasharg integer) RETURNS TABLE(version integer)
    LANGUAGE plpgsql
    AS $$
DECLARE
    VersionVar int := 0;
BEGIN

    INSERT INTO OrleansRemindersTable
    (
        ServiceId,
        GrainId,
        ReminderName,
        StartTime,
        Period,
        GrainHash,
        Version
    )
    SELECT
        ServiceIdArg,
        GrainIdArg,
        ReminderNameArg,
        StartTimeArg,
        PeriodArg,
        GrainHashArg,
        0
    ON CONFLICT (ServiceId, GrainId, ReminderName)
        DO UPDATE SET
            StartTime = excluded.StartTime,
            Period = excluded.Period,
            GrainHash = excluded.GrainHash,
            Version = OrleansRemindersTable.Version + 1
    RETURNING
        OrleansRemindersTable.Version INTO STRICT VersionVar;

    RETURN QUERY SELECT VersionVar AS versionr;

END
$$;


ALTER FUNCTION public.upsert_reminder_row(serviceidarg character varying, grainidarg character varying, remindernamearg character varying, starttimearg timestamp without time zone, periodarg bigint, grainhasharg integer) OWNER TO postgres;

--
-- TOC entry 207 (class 1255 OID 51257)
-- Name: writetostorage(integer, bigint, bigint, integer, character varying, character varying, character varying, integer, bytea, jsonb, xml); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.writetostorage(_grainidhash integer, _grainidn0 bigint, _grainidn1 bigint, _graintypehash integer, _graintypestring character varying, _grainidextensionstring character varying, _serviceid character varying, _grainstateversion integer, _payloadbinary bytea, _payloadjson jsonb, _payloadxml xml) RETURNS TABLE(newgrainstateversion integer)
    LANGUAGE plpgsql
    AS $$
DECLARE
     _newGrainStateVersion integer := _GrainStateVersion;
     RowCountVar integer := 0;

    BEGIN

    -- Grain state is not null, so the state must have been read from the storage before.
    -- Let's try to update it.
    --
    -- When Orleans is running in normal, non-split state, there will
    -- be only one grain with the given ID and type combination only. This
    -- grain saves states mostly serially if Orleans guarantees are upheld. Even
    -- if not, the updates should work correctly due to version number.
    --
    -- In split brain situations there can be a situation where there are two or more
    -- grains with the given ID and type combination. When they try to INSERT
    -- concurrently, the table needs to be locked pessimistically before one of
    -- the grains gets @GrainStateVersion = 1 in return and the other grains will fail
    -- to update storage. The following arrangement is made to reduce locking in normal operation.
    --
    -- If the version number explicitly returned is still the same, Orleans interprets it so the update did not succeed
    -- and throws an InconsistentStateException.
    --
    -- See further information at https://dotnet.github.io/orleans/Documentation/Core-Features/Grain-Persistence.html.
    IF _GrainStateVersion IS NOT NULL
    THEN
        UPDATE OrleansStorage
        SET
            PayloadBinary = _PayloadBinary,
            PayloadJson = _PayloadJson,
            PayloadXml = _PayloadXml,
            ModifiedOn = (now() at time zone 'utc'),
            Version = Version + 1

        WHERE
            GrainIdHash = _GrainIdHash AND _GrainIdHash IS NOT NULL
            AND GrainTypeHash = _GrainTypeHash AND _GrainTypeHash IS NOT NULL
            AND GrainIdN0 = _GrainIdN0 AND _GrainIdN0 IS NOT NULL
            AND GrainIdN1 = _GrainIdN1 AND _GrainIdN1 IS NOT NULL
            AND GrainTypeString = _GrainTypeString AND _GrainTypeString IS NOT NULL
            AND ((_GrainIdExtensionString IS NOT NULL AND GrainIdExtensionString IS NOT NULL AND GrainIdExtensionString = _GrainIdExtensionString) OR _GrainIdExtensionString IS NULL AND GrainIdExtensionString IS NULL)
            AND ServiceId = _ServiceId AND _ServiceId IS NOT NULL
            AND Version IS NOT NULL AND Version = _GrainStateVersion AND _GrainStateVersion IS NOT NULL;

        GET DIAGNOSTICS RowCountVar = ROW_COUNT;
        IF RowCountVar > 0
        THEN
            _newGrainStateVersion := _GrainStateVersion + 1;
        END IF;
    END IF;

    -- The grain state has not been read. The following locks rather pessimistically
    -- to ensure only on INSERT succeeds.
    IF _GrainStateVersion IS NULL
    THEN
        INSERT INTO OrleansStorage
        (
            GrainIdHash,
            GrainIdN0,
            GrainIdN1,
            GrainTypeHash,
            GrainTypeString,
            GrainIdExtensionString,
            ServiceId,
            PayloadBinary,
            PayloadJson,
            PayloadXml,
            ModifiedOn,
            Version
        )
        SELECT
            _GrainIdHash,
            _GrainIdN0,
            _GrainIdN1,
            _GrainTypeHash,
            _GrainTypeString,
            _GrainIdExtensionString,
            _ServiceId,
            _PayloadBinary,
            _PayloadJson,
            _PayloadXml,
           (now() at time zone 'utc'),
            1
        WHERE NOT EXISTS
         (
            -- There should not be any version of this grain state.
            SELECT 1
            FROM OrleansStorage
            WHERE
                GrainIdHash = _GrainIdHash AND _GrainIdHash IS NOT NULL
                AND GrainTypeHash = _GrainTypeHash AND _GrainTypeHash IS NOT NULL
                AND GrainIdN0 = _GrainIdN0 AND _GrainIdN0 IS NOT NULL
                AND GrainIdN1 = _GrainIdN1 AND _GrainIdN1 IS NOT NULL
                AND GrainTypeString = _GrainTypeString AND _GrainTypeString IS NOT NULL
                AND ((_GrainIdExtensionString IS NOT NULL AND GrainIdExtensionString IS NOT NULL AND GrainIdExtensionString = _GrainIdExtensionString) OR _GrainIdExtensionString IS NULL AND GrainIdExtensionString IS NULL)
                AND ServiceId = _ServiceId AND _ServiceId IS NOT NULL
         );

        GET DIAGNOSTICS RowCountVar = ROW_COUNT;
        IF RowCountVar > 0
        THEN
            _newGrainStateVersion := 1;
        END IF;
    END IF;

    RETURN QUERY SELECT _newGrainStateVersion AS NewGrainStateVersion;
END

$$;


ALTER FUNCTION public.writetostorage(_grainidhash integer, _grainidn0 bigint, _grainidn1 bigint, _graintypehash integer, _graintypestring character varying, _grainidextensionstring character varying, _serviceid character varying, _grainstateversion integer, _payloadbinary bytea, _payloadjson jsonb, _payloadxml xml) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 204 (class 1259 OID 51217)
-- Name: orleansmembershiptable; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orleansmembershiptable (
    deploymentid character varying(150) NOT NULL,
    address character varying(45) NOT NULL,
    port integer NOT NULL,
    generation integer NOT NULL,
    siloname character varying(150) NOT NULL,
    hostname character varying(150) NOT NULL,
    status integer NOT NULL,
    proxyport integer,
    suspecttimes character varying(8000),
    starttime timestamp(3) without time zone NOT NULL,
    iamalivetime timestamp(3) without time zone NOT NULL
);


ALTER TABLE public.orleansmembershiptable OWNER TO postgres;

--
-- TOC entry 203 (class 1259 OID 51210)
-- Name: orleansmembershipversiontable; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orleansmembershipversiontable (
    deploymentid character varying(150) NOT NULL,
    "timestamp" timestamp(3) without time zone DEFAULT timezone('utc'::text, now()) NOT NULL,
    version integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.orleansmembershipversiontable OWNER TO postgres;

--
-- TOC entry 202 (class 1259 OID 51202)
-- Name: orleansquery; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orleansquery (
    querykey character varying(64) NOT NULL,
    querytext character varying(8000) NOT NULL
);


ALTER TABLE public.orleansquery OWNER TO postgres;

--
-- TOC entry 206 (class 1259 OID 51243)
-- Name: orleansreminderstable; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orleansreminderstable (
    serviceid character varying(150) NOT NULL,
    grainid character varying(150) NOT NULL,
    remindername character varying(150) NOT NULL,
    starttime timestamp(3) without time zone NOT NULL,
    period bigint NOT NULL,
    grainhash integer NOT NULL,
    version integer NOT NULL
);


ALTER TABLE public.orleansreminderstable OWNER TO postgres;

--
-- TOC entry 205 (class 1259 OID 51234)
-- Name: orleansstorage; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orleansstorage (
    grainidhash integer NOT NULL,
    grainidn0 bigint NOT NULL,
    grainidn1 bigint NOT NULL,
    graintypehash integer NOT NULL,
    graintypestring character varying(512) NOT NULL,
    grainidextensionstring character varying(512),
    serviceid character varying(150) NOT NULL,
    payloadbinary bytea,
    payloadxml xml,
    payloadjson jsonb,
    modifiedon timestamp without time zone NOT NULL,
    version integer
);


ALTER TABLE public.orleansstorage OWNER TO postgres;

--
-- TOC entry 2851 (class 0 OID 51217)
-- Dependencies: 204
-- Data for Name: orleansmembershiptable; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 2850 (class 0 OID 51210)
-- Dependencies: 203
-- Data for Name: orleansmembershipversiontable; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 2849 (class 0 OID 51202)
-- Dependencies: 202
-- Data for Name: orleansquery; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.orleansquery (querykey, querytext) VALUES ('UpdateIAmAlivetimeKey', '
    -- This is expected to never fail by Orleans, so return value
    -- is not needed nor is it checked.
    SELECT * from update_i_am_alive_time(
        @DeploymentId,
        @Address,
        @Port,
        @Generation,
        @IAmAliveTime
    );
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('InsertMembershipVersionKey', '
    SELECT * FROM insert_membership_version(
        @DeploymentId
    );
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('InsertMembershipKey', '
    SELECT * FROM insert_membership(
        @DeploymentId,
        @Address,
        @Port,
        @Generation,
        @SiloName,
        @HostName,
        @Status,
        @ProxyPort,
        @StartTime,
        @IAmAliveTime,
        @Version
    );
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('UpdateMembershipKey', '
    SELECT * FROM update_membership(
        @DeploymentId,
        @Address,
        @Port,
        @Generation,
        @Status,
        @SuspectTimes,
        @IAmAliveTime,
        @Version
    );
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('MembershipReadRowKey', '
    SELECT
        v.DeploymentId,
        m.Address,
        m.Port,
        m.Generation,
        m.SiloName,
        m.HostName,
        m.Status,
        m.ProxyPort,
        m.SuspectTimes,
        m.StartTime,
        m.IAmAliveTime,
        v.Version
    FROM
        OrleansMembershipVersionTable v
        -- This ensures the version table will returned even if there is no matching membership row.
        LEFT OUTER JOIN OrleansMembershipTable m ON v.DeploymentId = m.DeploymentId
        AND Address = @Address AND @Address IS NOT NULL
        AND Port = @Port AND @Port IS NOT NULL
        AND Generation = @Generation AND @Generation IS NOT NULL
    WHERE
        v.DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('MembershipReadAllKey', '
    SELECT
        v.DeploymentId,
        m.Address,
        m.Port,
        m.Generation,
        m.SiloName,
        m.HostName,
        m.Status,
        m.ProxyPort,
        m.SuspectTimes,
        m.StartTime,
        m.IAmAliveTime,
        v.Version
    FROM
        OrleansMembershipVersionTable v LEFT OUTER JOIN OrleansMembershipTable m
        ON v.DeploymentId = m.DeploymentId
    WHERE
        v.DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('DeleteMembershipTableEntriesKey', '
    DELETE FROM OrleansMembershipTable
    WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
    DELETE FROM OrleansMembershipVersionTable
    WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('GatewaysQueryKey', '
    SELECT
        Address,
        ProxyPort,
        Generation
    FROM
        OrleansMembershipTable
    WHERE
        DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
        AND Status = @Status AND @Status IS NOT NULL
        AND ProxyPort > 0;
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('ReadFromStorageKey', '
    SELECT
        PayloadBinary,
        PayloadXml,
        PayloadJson,
        (now() at time zone ''utc''),
        Version
    FROM
        OrleansStorage
    WHERE
        GrainIdHash = @GrainIdHash
        AND GrainTypeHash = @GrainTypeHash AND @GrainTypeHash IS NOT NULL
        AND GrainIdN0 = @GrainIdN0 AND @GrainIdN0 IS NOT NULL
        AND GrainIdN1 = @GrainIdN1 AND @GrainIdN1 IS NOT NULL
        AND GrainTypeString = @GrainTypeString AND GrainTypeString IS NOT NULL
        AND ((@GrainIdExtensionString IS NOT NULL AND GrainIdExtensionString IS NOT NULL AND GrainIdExtensionString = @GrainIdExtensionString) OR @GrainIdExtensionString IS NULL AND GrainIdExtensionString IS NULL)
        AND ServiceId = @ServiceId AND @ServiceId IS NOT NULL
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('ClearStorageKey', '
    UPDATE OrleansStorage
    SET
        PayloadBinary = NULL,
        PayloadJson = NULL,
        PayloadXml = NULL,
        Version = Version + 1
    WHERE
        GrainIdHash = @GrainIdHash AND @GrainIdHash IS NOT NULL
        AND GrainTypeHash = @GrainTypeHash AND @GrainTypeHash IS NOT NULL
        AND GrainIdN0 = @GrainIdN0 AND @GrainIdN0 IS NOT NULL
        AND GrainIdN1 = @GrainIdN1 AND @GrainIdN1 IS NOT NULL
        AND GrainTypeString = @GrainTypeString AND @GrainTypeString IS NOT NULL
        AND ((@GrainIdExtensionString IS NOT NULL AND GrainIdExtensionString IS NOT NULL AND GrainIdExtensionString = @GrainIdExtensionString) OR @GrainIdExtensionString IS NULL AND GrainIdExtensionString IS NULL)
        AND ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND Version IS NOT NULL AND Version = @GrainStateVersion AND @GrainStateVersion IS NOT NULL
    Returning Version as NewGrainStateVersion
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('UpsertReminderRowKey', '
    SELECT * FROM upsert_reminder_row(
        @ServiceId,
        @GrainId,
        @ReminderName,
        @StartTime,
        @Period,
        @GrainHash
    );
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('ReadReminderRowsKey', '
    SELECT
        GrainId,
        ReminderName,
        StartTime,
        Period,
        Version
    FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND GrainId = @GrainId AND @GrainId IS NOT NULL;
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('ReadReminderRowKey', '
    SELECT
        GrainId,
        ReminderName,
        StartTime,
        Period,
        Version
    FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND GrainId = @GrainId AND @GrainId IS NOT NULL
        AND ReminderName = @ReminderName AND @ReminderName IS NOT NULL;
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('ReadRangeRows1Key', '
    SELECT
        GrainId,
        ReminderName,
        StartTime,
        Period,
        Version
    FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND GrainHash > @BeginHash AND @BeginHash IS NOT NULL
        AND GrainHash <= @EndHash AND @EndHash IS NOT NULL;
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('ReadRangeRows2Key', '
    SELECT
        GrainId,
        ReminderName,
        StartTime,
        Period,
        Version
    FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL
        AND ((GrainHash > @BeginHash AND @BeginHash IS NOT NULL)
        OR (GrainHash <= @EndHash AND @EndHash IS NOT NULL));
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('DeleteReminderRowKey', '
    SELECT * FROM delete_reminder_row(
        @ServiceId,
        @GrainId,
        @ReminderName,
        @Version
    );
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('DeleteReminderRowsKey', '
    DELETE FROM OrleansRemindersTable
    WHERE
        ServiceId = @ServiceId AND @ServiceId IS NOT NULL;
');
INSERT INTO public.orleansquery (querykey, querytext) VALUES ('WriteToStorageKey', '

        select * from WriteToStorage(@GrainIdHash, @GrainIdN0, @GrainIdN1, @GrainTypeHash, @GrainTypeString, @GrainIdExtensionString, @ServiceId, @GrainStateVersion, @PayloadBinary, CAST(@PayloadJson AS jsonb), CAST(@PayloadXml AS xml));
');


--
-- TOC entry 2853 (class 0 OID 51243)
-- Dependencies: 206
-- Data for Name: orleansreminderstable; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 2852 (class 0 OID 51234)
-- Dependencies: 205
-- Data for Name: orleansstorage; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 2714 (class 2606 OID 51209)
-- Name: orleansquery orleansquery_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orleansquery
    ADD CONSTRAINT orleansquery_key PRIMARY KEY (querykey);


--
-- TOC entry 2718 (class 2606 OID 51224)
-- Name: orleansmembershiptable pk_membershiptable_deploymentid; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orleansmembershiptable
    ADD CONSTRAINT pk_membershiptable_deploymentid PRIMARY KEY (deploymentid, address, port, generation);


--
-- TOC entry 2716 (class 2606 OID 51216)
-- Name: orleansmembershipversiontable pk_orleansmembershipversiontable_deploymentid; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orleansmembershipversiontable
    ADD CONSTRAINT pk_orleansmembershipversiontable_deploymentid PRIMARY KEY (deploymentid);


--
-- TOC entry 2721 (class 2606 OID 51247)
-- Name: orleansreminderstable pk_reminderstable_serviceid_grainid_remindername; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orleansreminderstable
    ADD CONSTRAINT pk_reminderstable_serviceid_grainid_remindername PRIMARY KEY (serviceid, grainid, remindername);


--
-- TOC entry 2719 (class 1259 OID 51240)
-- Name: ix_orleansstorage; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_orleansstorage ON public.orleansstorage USING btree (grainidhash, graintypehash);


--
-- TOC entry 2722 (class 2606 OID 51225)
-- Name: orleansmembershiptable fk_membershiptable_membershipversiontable_deploymentid; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orleansmembershiptable
    ADD CONSTRAINT fk_membershiptable_membershipversiontable_deploymentid FOREIGN KEY (deploymentid) REFERENCES public.orleansmembershipversiontable(deploymentid);


-- Completed on 2020-12-26 15:45:30

--
-- PostgreSQL database dump complete
--

