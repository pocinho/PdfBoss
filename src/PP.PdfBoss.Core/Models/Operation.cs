/*  PP.PdfBoss.Core\Models\Operation.cs
 *
 *  Copyright 2024 Paulo Pocinho.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

namespace PP.PdfBoss.Core.Models;

public class Operation
{
    protected bool _success;

    public string? Message { get; protected set; }

    public Operation()
    {
    }

    public Operation(bool success = false, string? message = null)
    {
        Message = message;
        _success = success;
    }

    public void SetSucceeded(string? message = null)
    {
        Message = message;
        _success = true;
    }

    public void SetFailed(string? message = null)
    {
        Message = message;
        _success = false;
    }

    public bool HasSucceeded
        => _success == true;

    public bool HasFailed
        => _success == false;
}

public class Operation<T> : Operation
{
    public T? Result { get; protected set; }

    public Operation()
    {
    }

    public Operation(T? result = default, bool success = false, string? message = null)
    {
        Result = result;
        Message = message;
        _success = success;
    }

    public void SetResult(T result)
    {
        Result = result;
        _success = true;
    }
}
