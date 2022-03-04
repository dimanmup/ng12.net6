import { gql } from 'apollo-angular';
import { Injectable } from '@angular/core';
import * as Apollo from 'apollo-angular';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: string;
  String: string;
  Boolean: boolean;
  Int: number;
  Float: number;
  /** The `DateTime` scalar represents an ISO-8601 compliant date time type. */
  DateTime: any;
};

export enum ApplyPolicy {
  AfterResolver = 'AFTER_RESOLVER',
  BeforeResolver = 'BEFORE_RESOLVER'
}

export type AuditEvent = {
  __typename?: 'AuditEvent';
  description?: Maybe<Scalars['String']>;
  httpStatusCode: Scalars['Int'];
  id: Scalars['Int'];
  localDateTime: Scalars['DateTime'];
  object: Scalars['String'];
  source?: Maybe<Scalars['String']>;
  sourceDevice?: Maybe<Scalars['String']>;
  sourceIPAddress?: Maybe<Scalars['String']>;
  utcDateTime: Scalars['DateTime'];
};

export type AuditEventCollectionSegment = {
  __typename?: 'AuditEventCollectionSegment';
  items?: Maybe<Array<AuditEvent>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type AuditEventFilterInput = {
  and?: InputMaybe<Array<AuditEventFilterInput>>;
  description?: InputMaybe<StringOperationFilterInput>;
  httpStatusCode?: InputMaybe<ComparableInt32OperationFilterInput>;
  id?: InputMaybe<ComparableInt32OperationFilterInput>;
  localDateTime?: InputMaybe<ComparableDateTimeOperationFilterInput>;
  object?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<AuditEventFilterInput>>;
  source?: InputMaybe<StringOperationFilterInput>;
  sourceDevice?: InputMaybe<StringOperationFilterInput>;
  sourceIPAddress?: InputMaybe<StringOperationFilterInput>;
  utcDateTime?: InputMaybe<ComparableDateTimeOperationFilterInput>;
};

export type AuditEventSortInput = {
  description?: InputMaybe<SortEnumType>;
  httpStatusCode?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  localDateTime?: InputMaybe<SortEnumType>;
  object?: InputMaybe<SortEnumType>;
  source?: InputMaybe<SortEnumType>;
  sourceDevice?: InputMaybe<SortEnumType>;
  sourceIPAddress?: InputMaybe<SortEnumType>;
  utcDateTime?: InputMaybe<SortEnumType>;
};

/** Information about the offset pagination. */
export type CollectionSegmentInfo = {
  __typename?: 'CollectionSegmentInfo';
  /** Indicates whether more items exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean'];
  /** Indicates whether more items exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean'];
};

export type ComparableDateTimeOperationFilterInput = {
  eq?: InputMaybe<Scalars['DateTime']>;
  gt?: InputMaybe<Scalars['DateTime']>;
  gte?: InputMaybe<Scalars['DateTime']>;
  in?: InputMaybe<Array<Scalars['DateTime']>>;
  lt?: InputMaybe<Scalars['DateTime']>;
  lte?: InputMaybe<Scalars['DateTime']>;
  neq?: InputMaybe<Scalars['DateTime']>;
  ngt?: InputMaybe<Scalars['DateTime']>;
  ngte?: InputMaybe<Scalars['DateTime']>;
  nin?: InputMaybe<Array<Scalars['DateTime']>>;
  nlt?: InputMaybe<Scalars['DateTime']>;
  nlte?: InputMaybe<Scalars['DateTime']>;
};

export type ComparableInt32OperationFilterInput = {
  eq?: InputMaybe<Scalars['Int']>;
  gt?: InputMaybe<Scalars['Int']>;
  gte?: InputMaybe<Scalars['Int']>;
  in?: InputMaybe<Array<Scalars['Int']>>;
  lt?: InputMaybe<Scalars['Int']>;
  lte?: InputMaybe<Scalars['Int']>;
  neq?: InputMaybe<Scalars['Int']>;
  ngt?: InputMaybe<Scalars['Int']>;
  ngte?: InputMaybe<Scalars['Int']>;
  nin?: InputMaybe<Array<Scalars['Int']>>;
  nlt?: InputMaybe<Scalars['Int']>;
  nlte?: InputMaybe<Scalars['Int']>;
};

export type Query = {
  __typename?: 'Query';
  auditEvents?: Maybe<AuditEventCollectionSegment>;
  auditUploadingsEvents?: Maybe<AuditEventCollectionSegment>;
};


export type QueryAuditEventsArgs = {
  order?: InputMaybe<Array<AuditEventSortInput>>;
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
  where?: InputMaybe<AuditEventFilterInput>;
};


export type QueryAuditUploadingsEventsArgs = {
  order?: InputMaybe<Array<AuditEventSortInput>>;
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
  where?: InputMaybe<AuditEventFilterInput>;
};

export enum SortEnumType {
  Asc = 'ASC',
  Desc = 'DESC'
}

export type StringOperationFilterInput = {
  and?: InputMaybe<Array<StringOperationFilterInput>>;
  contains?: InputMaybe<Scalars['String']>;
  endsWith?: InputMaybe<Scalars['String']>;
  eq?: InputMaybe<Scalars['String']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['String']>>>;
  ncontains?: InputMaybe<Scalars['String']>;
  nendsWith?: InputMaybe<Scalars['String']>;
  neq?: InputMaybe<Scalars['String']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['String']>>>;
  nstartsWith?: InputMaybe<Scalars['String']>;
  or?: InputMaybe<Array<StringOperationFilterInput>>;
  startsWith?: InputMaybe<Scalars['String']>;
};

export type AuditEventDescriptionByIdQueryVariables = Exact<{
  id: Scalars['Int'];
}>;


export type AuditEventDescriptionByIdQuery = { __typename?: 'Query', auditEvents?: Maybe<{ __typename?: 'AuditEventCollectionSegment', items?: Maybe<Array<{ __typename?: 'AuditEvent', description?: Maybe<string>, localDateTime: any }>> }> };

export type AuditEventsQueryVariables = Exact<{
  skip: Scalars['Int'];
  take: Scalars['Int'];
  oldestUtcDate: Scalars['DateTime'];
  uploadingsOnly?: Scalars['Boolean'];
}>;


export type AuditEventsQuery = { __typename?: 'Query', auditEvents?: Maybe<{ __typename?: 'AuditEventCollectionSegment', totalCount: number, items?: Maybe<Array<{ __typename?: 'AuditEvent', id: number, localDateTime: any, object: string, source?: Maybe<string>, httpStatusCode: number }>> }>, auditUploadingsEvents?: Maybe<{ __typename?: 'AuditEventCollectionSegment', totalCount: number, items?: Maybe<Array<{ __typename?: 'AuditEvent', id: number, localDateTime: any, object: string, source?: Maybe<string>, httpStatusCode: number }>> }> };

export const AuditEventDescriptionByIdDocument = gql`
    query AuditEventDescriptionById($id: Int!) {
  auditEvents(where: {id: {eq: $id}}) {
    items {
      description
      localDateTime
    }
  }
}
    `;

  @Injectable({
    providedIn: 'root'
  })
  export class AuditEventDescriptionByIdGQL extends Apollo.Query<AuditEventDescriptionByIdQuery, AuditEventDescriptionByIdQueryVariables> {
    document = AuditEventDescriptionByIdDocument;
    
    constructor(apollo: Apollo.Apollo) {
      super(apollo);
    }
  }
export const AuditEventsDocument = gql`
    query AuditEvents($skip: Int!, $take: Int!, $oldestUtcDate: DateTime!, $uploadingsOnly: Boolean! = false) {
  auditEvents(
    skip: $skip
    take: $take
    order: {id: DESC}
    where: {utcDateTime: {lt: $oldestUtcDate}}
  ) @skip(if: $uploadingsOnly) {
    items {
      id
      localDateTime
      object
      source
      httpStatusCode
    }
    totalCount
  }
  auditUploadingsEvents(
    skip: $skip
    take: $take
    order: {id: DESC}
    where: {utcDateTime: {lt: $oldestUtcDate}}
  ) @include(if: $uploadingsOnly) {
    items {
      id
      localDateTime
      object
      source
      httpStatusCode
    }
    totalCount
  }
}
    `;

  @Injectable({
    providedIn: 'root'
  })
  export class AuditEventsGQL extends Apollo.Query<AuditEventsQuery, AuditEventsQueryVariables> {
    document = AuditEventsDocument;
    
    constructor(apollo: Apollo.Apollo) {
      super(apollo);
    }
  }